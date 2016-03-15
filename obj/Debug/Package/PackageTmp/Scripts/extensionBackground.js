"use strict"

function updateIcon() {
    console.log(retrieveUserSchedule);
    function subscriber() {
    }
    retrieveUserSchedule.c
    setTimeout(updateIcon, 3000)
    chrome.browserAction.setIcon({ path: "img/Icons/logoYellow.png", });
}

var logos = ["logo.png", "logoYellow.png"]

function setYellowLogoIcon () {
    if(!!chrome.browserAction)
    {
        chrome.browserAction.setIcon({ path: "img/Icons/logoYellow.png", });
    }
    
}

function setGreenLogoIcon () {
    if(!!chrome.browserAction)
    {
        chrome.browserAction.setIcon({ path: "img/Icons/logoGreen.png", });
    }
    
}

function setOrangeLogoIcon () {
    if(!!chrome.browserAction)
    {
        chrome.browserAction.setIcon({ path: "img/Icons/logoOrange.png", });
    }
    
}

function setBlackLogoIcon() {
    if(!!chrome.browserAction)
    {
        chrome.browserAction.setIcon({ path: "img/Icons/logo.png", });
    }
}

function getLatestData(data) {
    var formattedScheduleData = StructuralizeNewData(data.Content)
    function sortSubCalendarEventsByEndTime(subCalA, subCalB) {
        return subCalA.SubCalEndDate - subCalB.SubCalEndDate
    }
    function  getSubEventWithEndTimeClosestToReftime(time, subEvents) {
        for(var i = 0; i<subEvents.length; i++)
        {
            var subEvent = subEvents[i]
            if(subEvent.SubCalEndDate.getTime() > time.getTime() )
            {
                return subEvent;
                break;
            }
        }

        return null;
    }


    formattedScheduleData.TotalSubEventList.sort(sortSubCalendarEventsByEndTime);
    function listenForStart(alarm){
        debugger
    }
    
    /// function updates the extension badge time span based on the provided refTime(a Date object)
    function updateTimeSpanBadge(refTime) {
        var now = new Date();
        var span = refTime.getTime() - now.getTime();
        var time = formatTimeString(span);
        var refreshCycle = time.refreshCycle;
        if (refreshCycle >= OneMinInMs) {
            refreshCycle /= OneMinInMs;
        } else {
            refreshCycle = OneSecondInMs/OneMinInMs
        }
        console.log(refreshCycle)
        chrome.browserAction.setBadgeText({text: time.formattedSpan});
        
        //cleans out previous async call before new async call.
        chrome.alarms.clear("RefreshTimeSpanForSubEvent")
        // if the span is less than a second dont bother making a refresh request
        if(span > 1000)
        {
            chrome.alarms.create("RefreshTimeSpanForSubEvent", {periodInMinutes:refreshCycle})
            var RefreshTimeSpanForSubEvent = function(alarm) {
                if(alarm.name === "RefreshTimeSpanForSubEvent") {
                    updateTimeSpanBadge(refTime)
                }
            }
            if(!updateTimeSpanBadge.isEnrolled)
            {
                chrome.alarms.onAlarm.addListener(RefreshTimeSpanForSubEvent);
                updateTimeSpanBadge.isEnrolled = true
            }
        }
        else {
            chrome.alarms.clear("RefreshTimeSpanForSubEvent")
            getNewData()
        }
    }
    // flag is set to true if updateTimeSpanBadge is added to the alarm listener. This initializes the flag to zero
    updateTimeSpanBadge.isEnrolled = false;

    var subEvent = getSubEventWithEndTimeClosestToReftime(new Date(), formattedScheduleData.TotalSubEventList);
    if(!!subEvent)
    {   
        var title = { title: subEvent.SubCalCalendarName};
        chrome.browserAction.setTitle(title)
        if (subEvent.SubCalStartDate.getTime() < (new Date()).getTime())
        {
            //setOrangeLogoIcon()
            chrome.browserAction.setBadgeBackgroundColor({ color: [255, 106, 0, 255] });

            
            updateTimeSpanBadge(subEvent.SubCalEndDate)
        } else { 
            if (subEvent.SubCalStartDate.getTime() > (new Date()).getTime())
            {
                //setGreenLogoIcon()
                chrome.browserAction.setBadgeBackgroundColor({ color: [135, 255, 221, 255] });
                updateTimeSpanBadge(subEvent.SubCalStartDate)
            }
        }
        setBlackLogoIcon()
    }
    else {
        setBlackLogoIcon()
    }
    /* function formats a provided time span to its most significant time elements. The return value is an object containing the timespan string and the refreshcycle in ms for the corresponding string.
    * So if it returns 20s (20 seconds)it returns the refresh to be 1000ms. Or if it is 2h (2hours) the refresh cycle will be 3600000ms
    */
    function formatTimeString(timeSpanInMs)
    {   
        var retValue = {cycleInMs: -1, string: ""}
        var retValueString = "";
        var refreshCycle = 0;

        //more than one day
        if(timeSpanInMs > OneDayInMs) 
        {
            retValueString = (Math.round(timeSpanInMs/OneDayInMs)) + "d"
            refreshCycle = OneDayInMs;
        }

        //less than one day
        if(timeSpanInMs < OneDayInMs) 
        {
            retValueString = (Math.round(timeSpanInMs/OneHourInMs)) + "h"
            refreshCycle = OneHourInMs;
        }

        //less than one hour
        if(timeSpanInMs < OneHourInMs) 
        {
            retValueString = (Math.round(timeSpanInMs/OneMinInMs)) + "m"
            refreshCycle = OneMinInMs
        }

        //less than one minute
        if(timeSpanInMs < OneMinInMs)
        {
            retValueString = (Math.round(timeSpanInMs/OneSecondInMs)) + "s"
            refreshCycle = OneSecondInMs
        }
        retValue = {refreshCycle: refreshCycle, formattedSpan: retValueString}
        
        return retValue
    }
    
}



retrieveUserSchedule.subscribeToBeforeRefresh(setYellowLogoIcon);
retrieveUserSchedule.subscribeToSuccessfulRefresh(getLatestData);

/*
var verifiedUser = GetCookieValue();
var preppePostdData = { UserName: verifiedUser.UserName, UserID: verifiedUser.UserID };
var url = "http://mytilerkid.azurewebsites.net/api/Schedule?UserName=jerbio&UserID=d350ba4d-fe0b-445c-bed6-b6411c2156b3&TimeZoneOffset=300&StartRange=1456870504304&EndRange=1456956904304"
retrieveUserSchedule(url, preppePostdData)*/
getNewData()