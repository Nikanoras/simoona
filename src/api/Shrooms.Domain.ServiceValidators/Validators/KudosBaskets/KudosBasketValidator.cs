using System;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Exceptions.Exceptions.KudosBaskets;

namespace Shrooms.Domain.ServiceValidators.Validators.KudosBaskets
{
    public class KudosBasketValidator : IKudosBasketValidator
    {
        public void CheckIfThereIsNoBasketYet(KudosBasketDto basket)
        {
            if (basket != null)
            {
                return;
            }

            throw new KudosBasketException(Resources.Widgets.KudosBasket.KudosBasket.KudosBasketNotExists);
        }

        public void CheckIfUserHasEnoughKudos(decimal remainingKudos, decimal donationAmount)
        {
            if (remainingKudos >= donationAmount)
            {
                return;
            }

            throw new KudosBasketException(Resources.Widgets.KudosBasket.KudosBasket.KudosBasketNotEnoughKudos);
        }

        public void CheckIfBasketIsActive(KudosBasket basket)
        {
            if (basket.IsActive)
            {
                return;
            }

            throw new KudosBasketException(Resources.Widgets.KudosBasket.KudosBasket.KudosBasketInactive);
        }

        public void CheckIfBasketAlreadyExists(bool basketExists)
        {
            if (!basketExists)
            {
                return;
            }

            throw new Exception("Kudos Basket already exists");
        }
    }
}
