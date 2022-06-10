﻿using Newtonsoft.Json;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.FilterPresets;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.ServiceValidators.Validators.FilterPresets;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using System;
using Shrooms.DataLayer.EntityModels.Models.Events;

namespace Shrooms.Domain.Services.FilterPresets
{
    public class FilterPresetService : IFilterPresetService
    {
        private static readonly SemaphoreSlim _filterPresetUpdateCreateLock = new SemaphoreSlim(1, 1);

        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<FilterPreset> _filterPresetDbSet;
        private readonly IDbSet<KudosType> _kudosTypeDbSet;
        private readonly IDbSet<Office> _officeDbSet;
        private readonly IDbSet<EventType> _eventTypeDbSet;

        private readonly IFilterPresetValidator _validator;

        public FilterPresetService(IUnitOfWork2 uow, IFilterPresetValidator validator)
        {
            _uow = uow;
            _validator = validator;

            _filterPresetDbSet = uow.GetDbSet<FilterPreset>();
            _kudosTypeDbSet = uow.GetDbSet<KudosType>();
            _officeDbSet = uow.GetDbSet<Office>();
            _eventTypeDbSet = uow.GetDbSet<EventType>();
        }

        public async Task<IEnumerable<FilterPresetDto>> GetPresetsForPageAsync(PageType type, int organizationId)
        {
            _validator.CheckIfPageTypeExists(type);

            return (await _filterPresetDbSet
                .Where(preset => preset.OrganizationId == organizationId &&
                                 preset.ForPage == type)
                .ToListAsync())
                .Select(preset => new FilterPresetDto
                {
                    Id = preset.Id,
                    Name = preset.Name,
                    IsDefault = preset.IsDefault,
                    Filters = JsonConvert.DeserializeObject<IEnumerable<FilterPresetItemDto>>(preset.Preset)
                });
        }

        public async Task UpdateAsync(AddEditDeleteFilterPresetDto updateDto)
        {
            await _filterPresetUpdateCreateLock.WaitAsync();

            try
            {
                _validator.CheckIfMoreThanOneDefaultPresetExists(updateDto);
                _validator.CheckIfUniqueNames(updateDto);

                await UpdatePresetsAsync(updateDto);
                await DeleteAsync(updateDto);

                CreatePresets(updateDto);

                await _uow.SaveChangesAsync(updateDto.UserOrg.UserId);
            }
            finally
            {
                _filterPresetUpdateCreateLock.Release();
            }
        }

        public async Task<IEnumerable<FiltersDto>> GetFiltersAsync(FilterType[] filterTypes, int organizationId)
        {
            _validator.CheckIfFilterTypesContainsDuplicates(filterTypes);
            _validator.CheckIfFilterTypesAreValid(filterTypes);

            var filtersDtos = new List<FiltersDto>();

            foreach (var type in filterTypes)
            {
                var filters = await GetFiltersQuery(type).ToListAsync();
                var filtersDto = new FiltersDto
                {
                    FilterType = type,
                    Filters = filters,
                };

                filtersDtos.Add(filtersDto);
            }

            return filtersDtos;
        }

        private IQueryable<FilterDto> GetFiltersQuery(FilterType filterType)
        {
            return filterType switch
            {
                FilterType.Offices => _officeDbSet.Select(filter => new FilterDto
                {
                    Id = filter.Id,
                    Name = filter.Name
                }),
                FilterType.Kudos => _kudosTypeDbSet.Select(filter => new FilterDto
                {
                    Id = filter.Id,
                    Name = filter.Name
                }),
                FilterType.Events => _eventTypeDbSet.Select(filter => new FilterDto
                {
                    Id = filter.Id,
                    Name = filter.Name
                }),
                _ => throw new NotImplementedException()
            };
        }

        private async Task DeleteAsync(AddEditDeleteFilterPresetDto updateDto)
        {
            if (!updateDto.PresetsToRemove.Any())
            {
                return;
            }

            var presets = await _filterPresetDbSet
                .Where(preset => 
                    updateDto.PresetsToRemove.Contains(preset.Id) &&
                    preset.OrganizationId == updateDto.UserOrg.OrganizationId)
                .ToListAsync();

            foreach (var preset in presets)
            {
                _filterPresetDbSet.Remove(preset);
            }
        }

        private void CreatePresets(AddEditDeleteFilterPresetDto updateDto)
        {
            if (!updateDto.PresetsToAdd.Any())
            {
                return;
            }

            var timestamp = DateTime.UtcNow;

            foreach (var preset in updateDto.PresetsToAdd)
            {
                var newPreset = new FilterPreset
                {
                    OrganizationId = updateDto.UserOrg.OrganizationId,
                    CreatedBy = updateDto.UserOrg.UserId,
                    Name = preset.Name,
                    IsDefault = preset.IsDefault,
                    ForPage = updateDto.PageType,
                    Modified = timestamp,
                    Created = timestamp,
                    Preset = JsonConvert.SerializeObject(preset.Filters)
                };

                _filterPresetDbSet.Add(newPreset);
            }
        }

        private async Task UpdatePresetsAsync(AddEditDeleteFilterPresetDto updateDto)
        {
            if (!updateDto.PresetsToUpdate.Any())
            {
                return;
            }

            var presetsToUpdateIds = updateDto.PresetsToUpdate.Select(preset => preset.Id)
                .ToList();

            var existingPresets = await _filterPresetDbSet
                .Where(preset => presetsToUpdateIds.Contains(preset.Id))
                .ToDictionaryAsync(preset => preset.Id, preset => preset);

            _validator.CheckIfCountsAreEqual(presetsToUpdateIds, existingPresets);

            foreach (var preset in updateDto.PresetsToUpdate)
            {
                if (!existingPresets.TryGetValue(preset.Id, out var presetToUpdate))
                {
                    continue;
                }

                presetToUpdate.IsDefault = preset.IsDefault;

                if (presetToUpdate.IsDefault)
                {
                    await ChangeCurrentDefaultFilterToNonDefaultAsync(presetToUpdate);
                }

                presetToUpdate.Name = preset.Name;
                presetToUpdate.Preset = JsonConvert.SerializeObject(preset.Filters);
            }
        }

        private async Task ChangeCurrentDefaultFilterToNonDefaultAsync(FilterPreset preset)
        {
            // Find current default filter
            var defaultFilter = await _filterPresetDbSet
                .FirstOrDefaultAsync(filter => filter.IsDefault && 
                                     filter.ForPage == preset.ForPage &&
                                     filter.OrganizationId == preset.OrganizationId);

            // Change to non default
            if (defaultFilter != null)
            {
                defaultFilter.IsDefault = false;
            }
        }
    }
}
