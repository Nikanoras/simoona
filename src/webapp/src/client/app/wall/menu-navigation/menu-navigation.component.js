(function () {
    'use strict';

    angular
        .module('simoonaApp.Wall')
        .component('aceWallMenuNavigation', {
            templateUrl: 'app/wall/menu-navigation/menu-navigation.html',
            controller: wallMenuNavigationController,
            controllerAs: 'vm'
        });

    wallMenuNavigationController.$inject = [
        '$state',
        'wallService',
        'appConfig',
        'Analytics',
        'errorHandler',
        'notificationHub',
        'leftMenuService'
    ];

    function wallMenuNavigationController($state, wallService, appConfig, Analytics, errorHandler, notificationHub, leftMenuService) {
        /* jshint validthis: true */
        const vm = this;

        vm.wallServiceData = wallService.wallServiceData;

        vm.state = $state;

        vm.filterWall = filterWall;
        vm.getAllWalls = getAllWalls;
        vm.isMenuExpanded = vm.state.params.wall || vm.state.includes('Root.WithOrg.Client.Wall.Item.Feed') || vm.state.includes('Root.WithOrg.Client.Wall.All');

        init();

        ////////

        function init() {
            notificationHub.initHubConnection();

            if (!$state.includes(appConfig.homeStateName)) {
                wallService.getChosenWallList(false);
            }
        }


        function filterWall(wallId) {
            Analytics.trackEvent('Filter wall', 'Wall id: ' + wallId, 'From state: ' + $state.current.name);
            wallService.wallServiceData.wallHeader = null;
            if (!!wallId) {
                vm.wallServiceData.isWallHeaderLoading = true;
                wallService.getWallDetails(wallId);
            }

            $state.params.wall = wallId;
            $state.params.search = null;
            $state.params.post = null;

            angular.element('#wall-search-input').val('');
            angular.element('.input-clear').addClass('ng-hide');

            if ($state.includes(appConfig.homeStateName)) {
                wallService.initWall(true, wallId);
            } else {
                $state.go(appConfig.homeStateName, {
                    search: null,
                    post: null,
                    wall: wallId
                });
            }
            leftMenuService.setStatus(false);
            document.getElementsByTagName('body')[0].classList.toggle('overflow');

        }

        function getAllWalls() {
            if ($state.includes('Root.WithOrg.Client.Wall.All')) {
                wallService.initWall(true, null);
            } else {
                $state.go('Root.WithOrg.Client.Wall.All');
            }
            leftMenuService.setStatus(false);
            document.getElementsByTagName('body')[0].classList.toggle('overflow');
        }
    }
})();
