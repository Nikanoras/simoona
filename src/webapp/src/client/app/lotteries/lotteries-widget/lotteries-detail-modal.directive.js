(function () {
    'use strict';

    angular
        .module('simoonaApp.Lotteries')
        .constant('lotteryImageSettings', {
            height: 165,
            width: 291,
        })
        .directive('aceLotteriesDetailModal', lotteryDetailModal)

        lotteryDetailModal.$inject = [
        '$uibModal'
    ];

    function lotteryDetailModal($uibModal) {
        var directive = {
            restrict: 'A',
            scope: {
                aceLotteriesDetailModal: '=?'
            },
            link: linkFunc
        };
        return directive;

        function linkFunc(scope, elem) {
            elem.bind('click', function () {
                $uibModal.open({
                    templateUrl: 'app/lotteries/lotteries-widget/lotteries-detail-modal.html',
                    controller: lotteriesDetailController,
                    controllerAs: 'vm',
                    resolve: {
                        currentLottery: function () {
                            return scope.aceLotteriesDetailModal;
                        }
                    }
                });
            });
        }
    }

    lotteriesDetailController.$inject = [
        '$uibModalInstance',
        'lotteryFactory',
        'currentLottery',
        'lotteryImageSettings',
        'notifySrv',
        'localeSrv'
    ];

    function lotteriesDetailController($uibModalInstance, lotteryFactory, currentLottery, lotteryImageSettings, notifySrv, localeSrv) {
        var vm = this;
        vm.lotteryImageSize = {
            w: lotteryImageSettings.width,
            h: lotteryImageSettings.height
        };

        vm.ticketCount = 0;
        vm.currentLottery = currentLottery;
        
        vm.localeSrv = localeSrv;
        vm.notifySrv = notifySrv;
        vm.cancel = cancel;
        vm.ticketUp = ticketUp;
        vm.ticketDown = ticketDown;
        vm.buyTickets = buyTickets;

        init();
        
        //////

        function init() {

            lotteryFactory.getLottery(currentLottery)
            .then(function(lottery){
                vm.lottery = lottery;
            });
        }
        function cancel() {
            $uibModalInstance.dismiss('cancel');
        }
        function ticketUp() {
            vm.ticketCount += 1;
        }
        function ticketDown() {
            if(vm.ticketCount > 0){
                vm.ticketCount -= 1;
            }
        }
        function buyTickets(){
            if(vm.ticketCount > 0)
            {
                vm.notifySrv.success(vm.localeSrv.formatTranslation('lotteries.hasBeenBought', { one: vm.ticketCount, two: vm.lottery.title }));
            }
            else
            {
                vm.notifySrv.error(vm.localeSrv.formatTranslation('lotteries.invalidTicketNumber'));
            }
            
        }
    }
})();
