/// <reference path="../Libs/angular.min.js" />
/// <reference path="../Libs/angular-route.min.js" />

(function (angular) {

    var app = angular.module("ttIdmApp", ['ngRoute', 'ttIdm', 'ttIdmUI', 'ttIdmUsers', 'ttIdmRoles']);
    function config(PathBase, $routeProvider) {
        $routeProvider
            .when("/", {
                controller: 'HomeCtrl',
                templateUrl: PathBase + '/assets/Templates.home.html'
            })
            .when("/logout", {
                templateUrl: PathBase + '/assets/Templates.home.html'
            })
            .when("/callback/:response", {
                templateUrl: PathBase + '/assets/Templates.message.html',
                controller: 'CallbackCtrl'
            })
            .when("/error", {
                templateUrl: PathBase + '/assets/Templates.message.html'
            })
            .otherwise({
                redirectTo: '/'
            });
    }
    config.$inject = ["PathBase", "$routeProvider"];
    app.config(config);

    function LayoutCtrl($rootScope, PathBase, idmApi, $location, $window, idmErrorService, ShowLoginButton) {
        $rootScope.PathBase = PathBase;
        $rootScope.layout = {};

        function removed() {
            idmErrorService.clear();
            $rootScope.layout.username = null;
            $rootScope.layout.links = null;
            $rootScope.layout.showLogout = ShowLoginButton; // TODO: logout button
            $rootScope.layout.showLogin = false; // TODO: login button
        }

        function load() {
            removed();

            idmApi.get().then(function (api) {
                $rootScope.layout.username = api.data.currentUser.username; // TODO: username
                $rootScope.layout.links = api.links;
            }, function (err) {
                idmErrorService.show(err);
            });
        }

        load();

        $rootScope.logout = function () {
            idmErrorService.clear();
            // TODO: logout functionality
            $location.path("/logout");
            if (ShowLoginButton !== false) {
                $window.location = PathBase + "/logout";
            }
        }
    }
    LayoutCtrl.$inject = ["$rootScope", "PathBase", "idmApi", "$location", "$window", "idmErrorService", "ShowLoginButton"];
    app.controller("LayoutCtrl", LayoutCtrl);

    function HomeCtrl(ShowLoginButton, $routeParams) {
        if (ShowLoginButton === false) { // TODO: Cleanup
        }
    }
    HomeCtrl.$inject = ["ShowLoginButton", "$routeParams"];
    app.controller("HomeCtrl", HomeCtrl);

    function CallbackCtrl($location, $rootScope, $routeParams, idmErrorService) {
        var hash = $routeParams.response;
        if (hash.charAt(0) === "&") {
            hash = hash.substr(1);
        }
    }
    CallbackCtrl.$inject = ["$location", "$rootScope", "$routeParams", "idmErrorService"];
    app.controller("CallbackCtrl", CallbackCtrl);
})(angular);
