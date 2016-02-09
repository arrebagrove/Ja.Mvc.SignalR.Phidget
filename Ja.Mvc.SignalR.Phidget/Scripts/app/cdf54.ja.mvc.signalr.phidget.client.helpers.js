/*
 * Author: José ALVAREZ
 * Date: 07/02/2015
 * Description: Helpers for this application.
 * File: ja.mvc.signalr.phidget.client.helpers.js
 */
//#region reference path.
/// <reference path="~/Views/Phidget/_ViewStart.cshtml" />
//#endregion

//#region CDF54.JA.MVC.SIGNALR.PHIDGET.CLIENT.HELPERS module.
CDF54.JA.MVC.SIGNALR.PHIDGET.CLIENT.HELPERS = (function () {
    'use strict';
    //
    // Private members
    //

    //
    // Public members
    //
    return {
        //
        // Public properties
        //

        //
        // Public methods
        //
        // get bootstrap version manual
        showBootstrapVersion: function (tag) {
            $('#' + tag).append("3.3.2");
        },
        // get bootstrap version need ~/Views/Phidget/_ViewStart.cshtml
        showBootstrapVersion1: function (tag) {
            var appPath = AppPath();
            $.get(appPath + "/Scripts/bootstrap.js", function (data) {
                var version = data.match(/v[.\d]+[.\d]/);
                //alert("V= " + version);
                $('#' + tag).append(version);
            });
        },
        // get jquery version
        showJqueryVersion: function (tag) {
            $('#' + tag).append($.fn.jquery);
        },
        // get signalR version
        showSignalRVersion: function (tag) {
            $('#' + tag).append($.signalR.version);
        },
        // get
    };// Public members
})();//CDF54.JA.MVC.SIGNALR.PHIDGET.CLIENT.HELPERS.
//#endregion