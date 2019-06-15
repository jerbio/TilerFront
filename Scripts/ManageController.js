"use strict"

function iniManager() {
    let submitButtonId = "submitDumpButton"
    let submitButton = getDomOrCreateNew(submitButtonId)
    //submitButton.Dom.onclick = requestScheduleDump
    submitButton.Dom.addEventListener("click", createScheduleDumpequest);
    let dumpRequestId = "DumpReferenceId"
    let dumpButton = getDomOrCreateNew(dumpRequestId)
    //submitButton.Dom.onclick = requestScheduleDump
    dumpButton.Dom.addEventListener("click", getDumpRequest);
}

function getDumpRequest(e, callBackSuccess, callBackFailure, callBackDone) {
    function sendGetRequest() {
        let dumpIdDom = getDomOrCreateNew("DumpReferenceText");
        let dumpId = dumpIdDom.Dom.innerHTML

        var TimeZone = new Date().getTimezoneOffset();
        var scheduleDumpData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, TimeZoneOffset: TimeZone, Id: dumpId };


        var URL = global_refTIlerUrl + "Schedule/DumpData";

        var HandleNEwPage = new LoadingScreenControl("Tiler is getting the data dump:)");
        scheduleDumpData.TimeZone = moment.tz.guess()
        HandleNEwPage.Launch();

        var exitSendMessage = function (data) {
            HandleNEwPage.Hide();
            //triggerUIUPdate();//hack alert
            global_ExitManager.triggerLastExitAndPop();
            //getRefreshedData();
        }

        var getDump = $.ajax({
            type: "GET",
            url: URL,
            data: scheduleDumpData,
            dataType: "json",
            success: function (data) {
                successDump(data, scheduleDumpData)
                exitSendMessage()
                if (callBackSuccess) {
                    callBackSuccess(data)
                }
            },
            error: function (data) {
                failureDump(data)
                if (callBackFailure) {
                    callBackFailure(data)
                }
                var NewMessage = "Ooops Tiler is having issues dumping your schedule. Please try again Later:X";
                var ExitAfter = {
                    ExitNow: true, Delay: 1000
                };
                HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exitSendMessage);
            }
        })

        if (callBackDone != undefined) {
            getDump.done(callBackDone);
        }
    }

    function successDump(data, scheduleDumpPost) {
        let filename = scheduleDumpPost.Id + " " + scheduleDumpPost.UserName + ".xml"
        var element = document.createElement('a');
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(data));
        element.setAttribute('download', filename);

        element.style.display = 'none';
        document.body.appendChild(element);

        element.click();

        document.body.removeChild(element);
    }

    function failureDump(data) {

    }


    sendGetRequest()
}

function createScheduleDumpequest(e, callBackSuccess, callBackFailure, callBackDone) {
    let dumpRefContainerId = "DumpReferenceId"
    $("#" + dumpRefContainerId).hide();
    function sendRequest() {
        let scheduleDumpIdNotes = "scheduleDumpNotes";
        let notesDom = getDomOrCreateNew(scheduleDumpIdNotes);
        let notesData = notesDom.value

        var TimeZone = new Date().getTimezoneOffset();
        var scheduleDumpData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, TimeZoneOffset: TimeZone, Notes: notesData };


        var URL = global_refTIlerUrl + "Schedule/DumpData";

        var HandleNEwPage = new LoadingScreenControl("Tiler is saving the data dump:)");
        scheduleDumpData.TimeZone = moment.tz.guess()
        HandleNEwPage.Launch();

        var exitSendMessage = function (data) {
            HandleNEwPage.Hide();
            //triggerUIUPdate();//hack alert
            global_ExitManager.triggerLastExitAndPop();
            //getRefreshedData();
        }

        var scheduleDumpRequest = $.ajax({
            type: "POST",
            url: URL,
            data: scheduleDumpData,
            dataType: "json",
            success: function (data) {
                successDump(data)
                exitSendMessage()
                if (callBackSuccess) {
                    callBackSuccess(data)
                }
            },
            error: function (data) {
                failureDump(data)
                if (callBackFailure) {
                    callBackFailure(data)
                }
                var NewMessage = "Ooops Tiler is having issues dumping your schedule. Please try again Later:X";
                var ExitAfter = {
                    ExitNow: true, Delay: 1000
                };
                HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exitSendMessage);
            }
        })

        if (callBackDone != undefined) {
            scheduleDumpRequest.done(callBackDone);
        }
    }

    function successDump(data) {
        $("#" + dumpRefContainerId).show();
        let textDom = getDomOrCreateNew("DumpReferenceText");
        $("#" + dumpRefContainerId).show();
        textDom.Dom.innerHTML= data.Content.Id
        function showSubmissionOverlay() {

        }
    }

    function failureDump(data) {

    }

    sendRequest()
}

iniManager();