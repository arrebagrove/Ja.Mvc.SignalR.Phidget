// [10004] ADD: cdf54.ja.mvc.signalr.phidget.client.app.js

/**
 * @Author: José ALVAREZ
 * @Date: 31/10/2015
 * @Description: PhidgetClient
 * http://falola.developpez.com/tutoriels/javascript/namespace/
 * http://thomas.junghans.co.za/frontendengineering/javascript-module-pattern/downloads/Javascript-Module-Pattern.pdf
 * @File: cdf54.ja.mvc.signalr.phidget.client.app.js
 */

//#region reference path.
// Directives de référence https://msdn.microsoft.com/fr-fr/library/bb385682.aspx#Script
// Une directive reference permet à Visual Studio d'établir une relation entre le script vous modifiez actuellement et d'autres scripts.
// La directive reference vous permet d'inclure un fichier de script dans le contexte de script du fichier de script actuel.
// Cela permet à IntelliSense de référencer des fonctions définies extérieurement, des types et des champs lors de l'écriture de code.
//  
// <reference path="~/Scripts/app/cdf54.ja.mvc.signalR.phidget.namespace.js" />
// <reference path="~/Scripts/app/cdf54.ja.mvc.signalR.phidget.helpers.js" />
// <reference path="~/Views/Phidget/_ViewStart.cshtml" />
//#endregion

var phidgetClientHub;
var phidgetServerData;
var phidgetServerAttached;
var isButton = false;

/** @description Exécuted on load.
 * TODO: Exécuted on load
 */
$(function () {
    // Declare a proxy to reference the hub. 
    phidgetClientHub = $.connection.phidgetMiddlewareServerHub;
    // Enable console logs if IsConsoleLoggingEnable is true in web.config
    $.connection.hub.logging = true;

    registerPhidgetClientMethods();

    // Start Hub
    $.connection.hub.start().done(function () {
        // registerEvents(phidgetClientHub);
        // After connection, PhidgetServer and PhidgetClient must call AfterConnect.
        var phidgetClient =
            {
                MachineName: PhidgetClientMachineName(),
                IPAddress: PhidgetClientIP(),
                SignalRClientTypeConnected: "PhidgetClient",

            };
        phidgetClientHub.server.afterConnect(phidgetClient)
    });
});

/** @description PhidgetClient RPC for PhidgetMiddlewareServer.
 * TODO: PhidgetClient RPC for PhidgetMiddlewareServer
 */
function registerPhidgetClientMethods() {
    // Called for the new PhidgetClient to populate his PhidgetServer list.
    phidgetClientHub.client.showPhidgetServerList = function (connectedPhidgetServers) {
        // TODO: Populate PhidgetServer list.
        alert("showPhidgetServerList: " + connectedPhidgetServers[0].MachineName);
        // Add All PhidgetServers
        for (i = 0; i < connectedPhidgetServers.length; i++) {
            AddPhidgetServer(connectedPhidgetServers[i]);
        }

    }
    // Called when a new PhidgetServer is connected.
    // PhidgetClient want only know  the new PhidgetServer connection, not the PhidgetClient connections.
    phidgetClientHub.client.onNewPhidgetServerConnect = function (phidgetServer) {
        // TODO: PhidgetClient must add this new PhidgetServer to his PhidgetServer list.
        alert("onNewPhidgetServerConnect: " + phidgetServer.MachineName);
        AddPhidgetServer(phidgetServer);

    }
    // Called by PhidgetMiddlewareServer to show data on PhidgetClient pushed by PhidgetServer.
    phidgetClientHub.client.show = function (data) {
        // TODO: Show data on PhidgetClient
        //alert("show: " + data.AntennaEnable + "/" + data.LedEnable + "/" + data.DigitalOutPuts[0] + "/" + data.DigitalOutPuts[1] + "/" + data.Tag);

        // Cache data.
        phidgetServerData = data;

        // Common
        $('#Attached').html(data.Attached);
        $('#Name').html(testUndefined(data.Name));
        $('#ID').html(testUndefined(data.ID));
        $('#Class').html(testUndefined(data.Class));
        $('#SerialNumber').html(testUndefined(data.SerialNumber));
        $('#Version').html(testUndefined(data.Version));
        $('#PhidgetModuleType').html(testUndefined(data.PhidgetModuleType));

        switch (data.PhidgetModuleType) {
            case "PhidgetRFID":
                // RFID
                $('#Outputs').html(testUndefined(data.Outputs));
                $('#Antenna').html((data.Antenna).toString()).attr('class', defineClass(data.Antenna));
                $('#LED').html((data.LED).toString()).attr('class', defineClass(data.LED));
                $('#KeyBoardOutPutEnable').html((data.KeyBoardOutPutEnable).toString()).attr('class', defineClass(data.KeyBoardOutPutEnable));
                $('#DigitalOutPuts0').html((data.DigitalOutPuts[0]).toString()).attr('class', defineClass(data.DigitalOutPuts[0]));
                $('#DigitalOutPuts1').html((data.DigitalOutPuts[1]).toString()).attr('class', defineClass(data.DigitalOutPuts[1]));
                $('#Tag').html(testUndefined(data.Tag));
                $('#Protocol').html(testUndefined(data.Protocol));
                $('#LastTag').html(testUndefined(data.LastTag));
                $('#LastTagProtocol').html(testUndefined(data.LastTagProtocol));
                break;
            case "PhidgetServo":
                // Servo
                $('#ServoNumber').html(testUndefined(data.ServoNumber));
                $('#Count').html(testUndefined(data.Count));
                $('#Type').html(testUndefined(data.Type[data.ServoNumber]));
                $('#Position').html(testUndefined(data.Position[data.ServoNumber]));
                var newposition = data.Position[data.ServoNumber] * 100 / data.PositionMax[data.ServoNumber];
                $('#progressPosition').css('width', newposition + '%').attr('aria-valuenow', newposition);
                $('#dialservo')
                    .val(data.Position[data.ServoNumber])
                    .trigger('change');
                $('#Degrees').html(testUndefined(data.Degrees));
                //$('#MinimumPulseWidth').html(testUndefined(data.MinimumPulseWidth));
                //$('#MaximumPulseWidth').html(testUndefined(data.MaximumPulseWidth));
                $('#PositionMax').html(testUndefined(data.PositionMax[data.ServoNumber]));
                $('#PositionMin').html(testUndefined(data.PositionMin)[data.ServoNumber]);
                $('#Engaged').html((data.Engaged[data.ServoNumber]).toString()).attr('class', defineClass(data.Engaged[data.ServoNumber]));
                break;
            case "PhidgetStarterKit":
                // StarterKit
                $('#Ratiometric').html((data.Ratiometric).toString());
                $('#DigitalInputs').html((data.DigitalInputCount));
                $('#DigitalOutputs').html((data.DigitalOutputCount));
                $('#AnalogInputs').html((data.AnalogInputCount));

                $('#AnalogInput0').html((data.AnalogInput[0]));
                var newAnalogInput0 = data.AnalogInput[0] * 100 / 1000;
                $('#progressAnalogInput0').css('width', newAnalogInput0 + '%').attr('aria-valuenow', newAnalogInput0);

                $('#AnalogInput1').html((data.AnalogInput[1]));
                var newAnalogInput1 = data.AnalogInput[1] * 100 / 1000;
                $('#progressAnalogInput1').css('width', newAnalogInput1 + '%').attr('aria-valuenow', newAnalogInput1);
                $('#dialrotation')
                    .val(data.AnalogInput[1])
                    .trigger('change');

                $('#AnalogInput2').html((data.AnalogInput[2]));
                var newAnalogInput2 = data.AnalogInput[2] * 100 / 1000;
                $('#progressAnalogInput2').css('width', newAnalogInput2 + '%').attr('aria-valuenow', newAnalogInput2);

                $('#AnalogInput3').html((data.AnalogInput[3]));
                var newAnalogInput3 = data.AnalogInput[3] * 100 / 1000;
                $('#progressAnalogInput3').css('width', newAnalogInput3 + '%').attr('aria-valuenow', newAnalogInput3);

                $('#AnalogInput4').html((data.AnalogInput[4]));
                var newAnalogInput4 = data.AnalogInput[4] * 100 / 1000;
                $('#progressAnalogInput4').css('width', newAnalogInput4 + '%').attr('aria-valuenow', newAnalogInput4);

                $('#AnalogInput5').html((data.AnalogInput[5]));
                $('#AnalogInput6').html((data.AnalogInput[6]));
                $('#AnalogInput7').html((data.AnalogInput[7]));

                $('#DigitalInput0').html((data.DigitalInput[0]).toString()).attr('class', defineClass(data.DigitalInput[0]));
                $('#DigitalInput1').html((data.DigitalInput[1]).toString()).attr('class', defineClass(data.DigitalInput[1]));
                $('#DigitalInput2').html((data.DigitalInput[2]).toString()).attr('class', defineClass(data.DigitalInput[2]));
                $('#DigitalInput3').html((data.DigitalInput[3]).toString()).attr('class', defineClass(data.DigitalInput[3]));
                $('#DigitalInput4').html((data.DigitalInput[4]).toString()).attr('class', defineClass(data.DigitalInput[4]));
                $('#DigitalInput5').html((data.DigitalInput[5]).toString()).attr('class', defineClass(data.DigitalInput[5]));
                $('#DigitalInput6').html((data.DigitalInput[6]).toString()).attr('class', defineClass(data.DigitalInput[6]));
                $('#DigitalInput7').html((data.DigitalInput[7]).toString()).attr('class', defineClass(data.DigitalInput[7]));

                $('#DigitalOutput0').html((data.DigitalOutput[0]).toString()).attr('class', defineClass(data.DigitalOutput[0]));
                $('#DigitalOutput1').html((data.DigitalOutput[1]).toString()).attr('class', defineClass(data.DigitalOutput[1]));
                $('#DigitalOutput2').html((data.DigitalOutput[2]).toString()).attr('class', defineClass(data.DigitalOutput[2]));
                $('#DigitalOutput3').html((data.DigitalOutput[3]).toString()).attr('class', defineClass(data.DigitalOutput[3]));
                $('#DigitalOutput4').html((data.DigitalOutput[4]).toString()).attr('class', defineClass(data.DigitalOutput[4]));
                $('#DigitalOutput5').html((data.DigitalOutput[5]).toString()).attr('class', defineClass(data.DigitalOutput[5]));
                $('#DigitalOutput6').html((data.DigitalOutput[6]).toString()).attr('class', defineClass(data.DigitalOutput[6]));
                $('#DigitalOutput7').html((data.DigitalOutput[7]).toString()).attr('class', defineClass(data.DigitalOutput[7]));
                break;
            default:
        }


    }
    // Called on PhidgetServer disconnection. 
    phidgetClientHub.client.onPhidgetServerDisconnect = function (phidgetServer) {
        // TODO: Remove this PhidgetServer from PhidgetServer list.
        alert("onPhidgetServerDisconnect :" + phidgetServer.MachineName);
        RemovePhidgetServer(phidgetServer);
    }
}
function defineClass(value) {
    var toreturn = 'badge';
    if (value) {
        toreturn = 'badge alert-danger';
    }
    return toreturn;

}
function testUndefined(value) {
    var toreturn = value;
    if (typeof value == 'undefined') {
        toreturn = "undefined";
    }
    return toreturn;
}
function loadservoknob() {
    $("#dialservo").knob({
        min: 0,
        max: 180,
        change: function (value) {
            console.log("change : " + value);
        },
        release: function (value) {
            //console.log(this.$.attr('value'));
            console.log("release : " + value + "isButton : " + isButton);
            //if(isButton == false)
            //{
            //    phidgetServerData.Position[phidgetServerData.ServoNumber] = value;
            //    phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
            //}
            //else {
            //    isButton = false;
            //}
        },
        cancel: function () {
            //console.log("cancel : ", this);
        },
        /*format : function (value) {
            return value + '%';
        },*/
        draw: function () {

            // "tron" case
            if (this.$.data('skin') == 'tron') {

                this.cursorExt = 0.3;

                var a = this.arc(this.cv)  // Arc
                    , pa                   // Previous arc
                    , r = 1;

                this.g.lineWidth = this.lineWidth;

                if (this.o.displayPrevious) {
                    pa = this.arc(this.v);
                    this.g.beginPath();
                    this.g.strokeStyle = this.pColor;
                    this.g.arc(this.xy, this.xy, this.radius - this.lineWidth, pa.s, pa.e, pa.d);
                    this.g.stroke();
                }

                this.g.beginPath();
                this.g.strokeStyle = r ? this.o.fgColor : this.fgColor;
                this.g.arc(this.xy, this.xy, this.radius - this.lineWidth, a.s, a.e, a.d);
                this.g.stroke();

                this.g.lineWidth = 2;
                this.g.beginPath();
                this.g.strokeStyle = this.o.fgColor;
                this.g.arc(this.xy, this.xy, this.radius - this.lineWidth + 1 + this.lineWidth * 2 / 3, 0, 2 * Math.PI, false);
                this.g.stroke();

                return false;
            }
        }
    });
};
function loadrotationknob() {
    $("#dialrotation").knob({
        min: 0,
        max: 1000,
    });
};

/** @description Events registration.
     * TODO: Events registration
     */
function registerEvents() {
    // User select a PhidgetServer on the PhidgetServer list,
    // hide previous hardware attached UI if convenient,
    // show UI for the hardware attached to the PhidgetServer selected by the user,
    // call OnPhidgetServerSelected to retreive data from PhidgetServer.
    //phidgetClientHub.server.onPhidgetServerSelected(phidgetServerSelected);

    // User manipulate activator on PCx.
    //phidgetClientHub.server.setByPhidgetClient(phidgetServer, data);

    $('#buttonToggleLED').click(function () {
        phidgetServerData.LED = !phidgetServerData.LED;
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonToggleAntenna').click(function () {
        phidgetServerData.Antenna = !phidgetServerData.Antenna;
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonToggleDigitalOutPuts0').click(function () {
        phidgetServerData.DigitalOutPuts[0] = !phidgetServerData.DigitalOutPuts[0];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonToggleDigitalOutPuts1').click(function () {
        phidgetServerData.DigitalOutPuts[1] = !phidgetServerData.DigitalOutPuts[1];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonServoPositionMin').click(function () {
        phidgetServerData.Position[phidgetServerData.ServoNumber] = phidgetServerData.PositionMin[phidgetServerData.ServoNumber];
        isButton = true;
        //$('.dial')
        //    .val(phidgetServerData.Position[phidgetServerData.ServoNumber])
        //    .trigger('change');
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonServoLess').click(function () {
        phidgetServerData.Position[phidgetServerData.ServoNumber] = --phidgetServerData.Position[phidgetServerData.ServoNumber];
        isButton = true;

        //$('.dial')
        //    .val(phidgetServerData.Position[phidgetServerData.ServoNumber])
        //    .trigger('change');
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonServoPlus').click(function () {
        phidgetServerData.Position[phidgetServerData.ServoNumber] = ++phidgetServerData.Position[phidgetServerData.ServoNumber];
        isButton = true;

        //$('.dial')
        //    .val(phidgetServerData.Position[phidgetServerData.ServoNumber])
        //    .trigger('change');
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonServoPositionMax').click(function () {
        phidgetServerData.Position[phidgetServerData.ServoNumber] = phidgetServerData.PositionMax[phidgetServerData.ServoNumber];
        isButton = true;

        //$('.dial')
        //    .val(phidgetServerData.Position[phidgetServerData.ServoNumber])
        //    .trigger('change');
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonEngaged').click(function () {
        phidgetServerData.Engaged[phidgetServerData.ServoNumber] = !phidgetServerData.Engaged[phidgetServerData.ServoNumber];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonRatiometric').click(function () {
        phidgetServerData.Ratiometric = !phidgetServerData.Ratiometric;
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonDigitalOutPut0').click(function () {
        phidgetServerData.DigitalOutput[0] = !phidgetServerData.DigitalOutput[0];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });
    $('#buttonDigitalOutPut1').click(function () {
        phidgetServerData.DigitalOutput[1] = !phidgetServerData.DigitalOutput[1];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });

    $('#buttonDigitalOutPut2').click(function () {
        phidgetServerData.DigitalOutput[2] = !phidgetServerData.DigitalOutput[2];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });

    $('#buttonDigitalOutPut3').click(function () {
        phidgetServerData.DigitalOutput[3] = !phidgetServerData.DigitalOutput[3];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });

    $('#buttonDigitalOutPut4').click(function () {
        phidgetServerData.DigitalOutput[4] = !phidgetServerData.DigitalOutput[4];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });

    $('#buttonDigitalOutPut5').click(function () {
        phidgetServerData.DigitalOutput[5] = !phidgetServerData.DigitalOutput[5];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });

    $('#buttonDigitalOutPut6').click(function () {
        phidgetServerData.DigitalOutput[6] = !phidgetServerData.DigitalOutput[6];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });

    $('#buttonDigitalOutPut7').click(function () {
        phidgetServerData.DigitalOutput[7] = !phidgetServerData.DigitalOutput[7];
        phidgetClientHub.server.setByPhidgetClient(phidgetServerAttached, phidgetServerData);
    });


}
/** @description Add PhidgetServer to the list.
 * @param {phidgetServer} phidgetServer to add. product
 */
function AddPhidgetServer(phidgetServer) {
    var code = "";
    var data = {
        connectionId: phidgetServer.ConnectionId,
        machineName: phidgetServer.MachineName,
        phidgetModuleTypeAttached: phidgetServer.PhidgetModuleTypeAttached,
    }
    data.theClass = "alert alert-info";
    data.theStyle = "cursor: pointer";
    data.theTitle = "Click to select PhidgetServer";
    data.imagePath = AppPath() + '/Content/' + phidgetServer.PhidgetModuleTypeAttached + '.jpg'
    code = $('#new-addphidgetserver-template').tmpl(data);
    $(code).on("click", function () {
        // User select a PhidgetServer on the PhidgetServer list,
        // hide previous hardware attached UI if convenient,
        // show UI for the hardware attached to the PhidgetServer selected by the user,
        // call OnPhidgetServerSelected to retreive data from PhidgetServer. 
        var phidgetServerSelected = {
            ConnectionId: $(this).attr('id'),
            MachineName: document.getElementById('MachineName').value,
            PhidgetModuleTypeAttached: document.getElementById('PhidgetModuleTypeAttached-' + $(this).attr('id')).value,
        }
        // TODO: phidgetClientHub
        phidgetServerAttached = phidgetServerSelected;
        var codeUI = $('#new-PhidgetCommon-template').tmpl();
        $("#divPhidgetClientUI").empty();
        $("#divPhidgetClientUI").append(codeUI);
        switch (phidgetServerAttached.PhidgetModuleTypeAttached) {
            case "PhidgetRFID":
                // RFID
                codeUI = $('#new-PhidgetRFID-template').tmpl();
                $("#divPhidgetClientUI").append(codeUI);
                break;
            case "PhidgetServo":
                // Servo
                codeUI = $('#new-PhidgetServo-template').tmpl();
                $("#divPhidgetClientUI").append(codeUI);
                loadservoknob();
                break;
            case "PhidgetStarterKit":
                // StarterKit
                codeUI = $('#new-PhidgetStarterKit-template').tmpl();
                $("#divPhidgetClientUI").append(codeUI);
                loadrotationknob();
                break;
            default:
        }
        phidgetClientHub.server.onPhidgetServerSelected(phidgetServerSelected);
        registerEvents();
        // Listen DOM changes
        alert("onPhidgetServerSelected: " + phidgetServerSelected.MachineName);
    });
    $("#divphidgetserver").append(code);
    // Using jQuery to scroll to the bottom of #panelPhidgetServer DIV.
    var height = $('#panelPhidgetServer')[0].scrollHeight;
    $('#panelPhidgetServer').scrollTop(height);
}
/** @description Remove PhidgetServer to the list.
 * @param {phidgetServer} phidgetServer to remove.
 */
function RemovePhidgetServer(phidgetServer) {
    if ($('#' + phidgetServer.ConnectionId).length > 0) {
        $("#divPhidgetClientUI").empty();
        $('#' + phidgetServer.ConnectionId).remove();
        var disc = $('<div class="disconnect">"' + phidgetServer.MachineName + '" removed.</div>');
        $(disc).hide();
        $('#divphidgetserver').prepend(disc);
        $(disc).fadeIn(200).delay(2000).fadeOut(200);
    }
}
