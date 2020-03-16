"use strict"

function iniManager() {
    let submitButtonId = "submitDumpButton"
    let submitButton = getDomOrCreateNew(submitButtonId)
    //submitButton.Dom.onclick = requestScheduleDump
    submitButton.Dom.addEventListener("click", createScheduleDumpequest);
    let dumpRequestId = "DumpReferenceId"
    let dumpButton = getDomOrCreateNew(dumpRequestId)

    let submitLogHistoryButtonId = "submitLogHistoryDump"
    let submitLogHistoryButton = getDomOrCreateNew(submitLogHistoryButtonId);
    createSubmitLogHistory(submitLogHistoryButton);
}

function getDumpRequest(e, callBackSuccess, callBackFailure, callBackDone) {
    function sendGetRequest() {
        let dumpIdDom = getDomOrCreateNew("DumpReferenceText");
        let dumpId = dumpIdDom.Dom.innerHTML

        var TimeZone = new Date().getTimezoneOffset();
        var scheduleDumpData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, TimeZoneOffset: TimeZone, Id: dumpId };


        var URL = global_refTIlerUrl + "Schedule/DumpData";

        //var HandleNEwPage = new LoadingScreenControl("Tiler is getting the data dump:)");
        //HandleNEwPage.Launch();
        scheduleDumpData.TimeZone = moment.tz.guess()
        preSendRequestWithLocation(scheduleDumpData);

        let params = jQuery.param(scheduleDumpData);
        let fullUrl = URL + "?" + params
        let hrefId = "dumpHref"
        let dumpButtonHref = getDomOrCreateNew(hrefId)
        dumpButtonHref.setAttribute("href", fullUrl)
        dumpButtonHref.setAttribute("download", dumpId)
        dumpButtonHref.innerHTML = dumpId
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

function createSubmitLogHistory(dowloadButton) {
    function bindDataHistoryLogClick() {
        let dayCount = 14;
        let TimeZone = new Date().getTimezoneOffset();
        let end = new Date();
        end.setHours(0,0,0,0);
        end.setDate(end.getDate() + (1));
        
        
        let start = new Date(end.getTime());
        start.setDate(start.getDate() - (dayCount));
        
        let dataHistoryDumpData = {
            UserName: UserCredentials.UserName,
            UserID: UserCredentials.ID,
            TimeZoneOffset: TimeZone,
            UserActivityType: "InternalUpdate",
            start: start.getTime(),
            end: end.getTime()

        };


        var URL = global_refTIlerUrl + "DataHistory/User";
        preSendRequestWithLocation(dataHistoryDumpData);

        let params = jQuery.param(dataHistoryDumpData);
        let fullUrl = URL + "?" + params
        dowloadButton.setAttribute("href", fullUrl);



        // var HandleNEwPage = new LoadingScreenControl("Tiler is generating the data histoy dump:)");
        // dataHistoryDumpData.TimeZone = moment.tz.guess()
        // preSendRequestWithLocation(dataHistoryDumpData);
        // HandleNEwPage.Launch();

        // var exitSendMessage = function (data) {
        //     HandleNEwPage.Hide();
        //     //triggerUIUPdate();//hack alert
        //     global_ExitManager.triggerLastExitAndPop();
        //     //getRefreshedData();
        // }

        // var scheduleDumpRequest = $.ajax({
        //     type: "GET",
        //     url: URL,
        //     data: dataHistoryDumpData,
        //     dataType: "json",
        //     success: function (data) {
        //         successDump(data)
        //         exitSendMessage()
        //         if (callBackSuccess) {
        //             callBackSuccess(data)
        //         }
        //     },
        //     error: function (data) {
        //         failureDump(data)
        //         if (callBackFailure) {
        //             callBackFailure(data)
        //         }
        //         var NewMessage = "Ooops Tiler is having issues dumping your schedule. Please try again Later:X";
        //         var ExitAfter = {
        //             ExitNow: true, Delay: 1000
        //         };
        //         HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exitSendMessage);
        //     }
        // })

        // if (callBackDone != undefined) {
        //     scheduleDumpRequest.done(callBackDone);
        // }
    }

    bindDataHistoryLogClick();
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
        preSendRequestWithLocation(scheduleDumpData);
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
        let dumpId = data.Content.Id

        var TimeZone = new Date().getTimezoneOffset();
        var scheduleDumpData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, TimeZoneOffset: TimeZone, Id: dumpId };


        var URL = global_refTIlerUrl + "Schedule/DumpData";
        scheduleDumpData.TimeZone = moment.tz.guess()
        preSendRequestWithLocation(scheduleDumpData);

        let params = jQuery.param(scheduleDumpData);
        let fullUrl = URL + "?" + params
        let hrefId = "dumpHref"
        let dumpButtonHref = getDomOrCreateNew(hrefId)
        dumpButtonHref.setAttribute("href", fullUrl)
        dumpButtonHref.setAttribute("download", dumpId)
        dumpButtonHref.innerHTML = dumpId


    }

    function failureDump(data) {

    }

    sendRequest()
}

iniManager();