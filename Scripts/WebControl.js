"use strict"
var DisableRegistration = false;
var Debug = false;
var DebugLocal = true;

//var global_refTIlerUrl = "http://localhost:53201/api/";
//var global_refTIlerUrl = "http://tilersmart.azurewebsites.net/api/";
var global_refTIlerUrl = window.location.origin + "/api/";
var global_PositionCoordinate = { Latitude: 40.0274, Longitude: -105.2519, isInitialized: false, Message: "Uninitialized" };;
var global_sleepTimeline = []
var UserTheme = { Light: new Theme("Light"), Dark: new Theme("Dark") };
var CurrentTheme = UserTheme.Light;
var UserCredentials;
try
{
    UserCredentials=  RetrieveUserCredentials();
}
catch(e)
{
    UserCredentials= { UserName: "", ID: "", Name: "" };
}

let pageNotifications = new PageNotification(UserCredentials.ID);
pageNotifications.authenticateNotification();

var global_DictionaryOfSubEvents = {};
var global_RemovedElemnts = {};

var OneDayInMs = 86400000;
var OneWeekInMs = OneDayInMs * 7;
var FourWeeksInMs = OneWeekInMs * 4;
var OneHourInMs = 3600000;
var OneYearInMs = 365 * OneDayInMs;
var OneMinInMs = 60000;
var OneSecondInMs = 1000;
var TenMinInMs = 600000;


var HeightOfCalendar = 720;
var WidthOfDay = 100;
var HeightPerHour = HeightOfCalendar / 24;
var TestStart = new Date(3000, 12, 10)
var TestEnd = new Date(3000, 12, 11)
var difference=TestEnd-TestStart;
var NumberOfCalendarWeeks=.5;
var CalendarStartRange = new Date(2014,1,23,0,0,0,0);
var WeekDays = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
var Months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
var CurrentWeekIndex=3;
var CurrentStartOfWeek = CalendarStartRange;
var CurrentFullSchedule = [new Date(CurrentStartOfWeek.getTime() + (0 * 24 * 60 * 60 * 1000)), new Date(CurrentStartOfWeek.getTime() + (1 * 24 * 60 * 60 * 1000)), new Date(CurrentStartOfWeek.getTime() + (2 * 24 * 60 * 60 * 1000)), new Date(CurrentStartOfWeek.getTime() + (3 * 24 * 60 * 60 * 1000)), new Date(CurrentStartOfWeek.getTime() + (4 * 24 * 60 * 60 * 1000)), new Date(CurrentStartOfWeek.getTime() + (5 * 24 * 60 * 60 * 1000)), new Date(CurrentStartOfWeek.getTime() + (6 * 24 * 60 * 60 * 1000))]
var Day = CalendarStartRange.getDay();
var Global_DSTIncrement = 0;

var defaultColorClass = { cssClass: "defaultColorOrb", r: 255, g: 255, b: 0, a: 1,Selection:0 };
var oriClass = { cssClass: "oriOrb", r: 135, g: 255, b: 221, a: 1, Selection: 1 };
var storyClass = { cssClass: "storyOrb", r: 38, g: 128, b: 255, a: 1, Selection: 2 };
var redClass = { cssClass: "redOrb", r: 255, g: 0, b: 0, a: 1, Selection: 3 };
var greenClass = { cssClass: "greenOrb", r: 0, g: 255, b: 0, a: 1, Selection: 4 };
var SkeleClass = { cssClass: "skeleOrb", r: 125, g: 33, b: 255, a: 1, Selection: 5 };
var mariClass = { cssClass: "mariOrb", r: 255, g: 160, b: 237, a: 1, Selection: 6 };
var purpleClass = { cssClass: "purpleOrb", r: 255, g: 0, b: 255, a: 1, Selection: 7 };
var ldonBoysClass = { cssClass: "ldonBoysOrb", r: 255, g: 103, b: 61, a: 1, Selection: 8 };

var global_AllColorClasses = [defaultColorClass, oriClass, storyClass, redClass, greenClass, purpleClass, SkeleClass, mariClass, ldonBoysClass]
var global_ExitManager = new OutOfFocusManager();
Date.prototype.stdTimezoneOffset = function () {
    var jan = new Date(this.getFullYear(), 0, 1);
    var jul = new Date(this.getFullYear(), 6, 1);
    return Math.max(jan.getTimezoneOffset(), jul.getTimezoneOffset());
}

var myClickManager = new OutClickManager();

Date.prototype.dst = function () {
    return this.getTimezoneOffset() < this.stdTimezoneOffset();
}

var googleAPiKey = "AIzaSyAOnexWFxnoQ6nQI7p64lyR8YgXwB4qRvU";//Debug ? "AIzaSyAeFh3yjsRCmTEL1ujbaA_CBk_r_LUNPY8" : "AIzaSyAd2zKjpB7lw4FS41oLzC2SPwtCP6HXHoc";




var global_TimeZone_ms = new Date().getTimezoneOffset()*60000;

function SetCookie(CookieValue)
{
    var CookieName = "TilerCaluserWaggy"
    var ExpiryDate = new Date();
    ExpiryDate.setTime(ExpiryDate.getTime() + 2000 * 24 * 60 * 60 * 1000);
    var ExpiryDateString = "; expires=" + ExpiryDate.toGMTString();
    document.cookie = CookieName + "=" + encodeURI(CookieValue) + ExpiryDateString + "path=/";
}
function GetCookieValue()//verifies that user has cookies
{
    var CookieName = "TilerCaluserWaggy";
    var CookieValue = "";//= JSON.parse(CookieValue);
    var DocumentCookie = " " + document.cookie + ";";
    var CookieSearchStr = " " + CookieName + "=";
    var CookieStartPosition = DocumentCookie.indexOf(CookieSearchStr);
    var CookieEndPosition;

    if (CookieStartPosition != -1) {
        CookieStartPosition += CookieSearchStr.length;
        CookieEndPosition = DocumentCookie.indexOf(";", CookieStartPosition);
        CookieValue = decodeURI(DocumentCookie.substring(CookieStartPosition, CookieEndPosition));
        CookieValue = JSON.parse(CookieValue);
    }
    if (CookieValue == "")
    {
        if (Debug) {
        ///*
            CookieValue = { UserName: "jerbio", UserID: "d350ba4d-fe0b-445c-bed6-b6411c2156b3",FullName:"Jerome" }
        //*/
        /*
        CookieValue = {};
        CookieValue.UserName = "jackostalk";
        CookieValue.UserID = "9c255a35-098c-417a-9a3a-c8b9e59b7f10";
        //*/
        }
    }

    return CookieValue;
}

function getBearerToken() {
    let localStorage = window.localStorage;
    let retValue = localStorage.getItem('bearerAccessToken');
    return retValue;
}



function initializeUserLocation(onSuccessLocationRetrieval, onfailure) {
    global_PositionCoordinate.Message ="Initializing"
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(populatePosition, showError);
    } else {
        if ((!!onSuccessLocationRetrieval) && (typeof (onSuccessLocationRetrieval) === "function"))
        {
            global_PositionCoordinate.Message = "Location services not suppported in browser"
            onfailure(global_PositionCoordinate.Message)
        }
    }

    function populatePosition(position) {
        global_PositionCoordinate.isInitialized = true;
        global_PositionCoordinate.Latitude = position.coords.latitude;
        global_PositionCoordinate.Longitude = position.coords.longitude;
        if ((!!onSuccessLocationRetrieval) && (typeof (onSuccessLocationRetrieval) === "function")) {
            onSuccessLocationRetrieval(global_PositionCoordinate);
        }
        global_PositionCoordinate.Message = "Done"
    }

    function showError(error)
    {
        switch (error.code)
        {
            case error.PERMISSION_DENIED:
                global_PositionCoordinate.Message = "User denied the request for Geolocation."
                break;
            case error.POSITION_UNAVAILABLE:
                global_PositionCoordinate.Message = "Location information is unavailable."
                break;
            case error.TIMEOUT:
                global_PositionCoordinate.Message = "The request to get user location timed out."
                break;
            case error.UNKNOWN_ERROR:
                global_PositionCoordinate.Message = "An unknown error occurred."
                break;
        }
        if ((!!onSuccessLocationRetrieval) && (typeof (onSuccessLocationRetrieval) === "function"))
        {
            onfailure(global_PositionCoordinate.Message)
        }
        
    }

}


function removeElement(id)
{
    var elem;
    if (typeof id === 'string') {
        return (elem = document.getElementById(id)).parentNode.removeChild(elem);
    }
    else
    {
        return (elem = id).parentNode.removeChild(elem);
    }

}


function triggerUndoPanel(UndoMessage)
{

    var UndoPanelContainerID = "UndoPanelContainer"
    var UndoPanelContainer = getDomOrCreateNew(UndoPanelContainerID);

    var UndoTextContainerID = "UndoTextContainer"
    var UndoTextContainer = getDomOrCreateNew(UndoTextContainerID, "span");
    UndoTextContainer.innerHTML = UndoMessage == null ? "Undo Last Act in ..." : UndoMessage;


    var UndoCountDownTextContainerID = "UndoCountDownTextContainer"
    var UndoCountDownTextContainer = getDomOrCreateNew(UndoCountDownTextContainerID, "span");
    
    

    UndoPanelContainer.appendChild(UndoTextContainer);
    UndoPanelContainer.appendChild(UndoCountDownTextContainer);
    UndoPanelContainer.onclick = clickUnDoPanel;

    var NumberOfSeconds = 10;
    showUndoPanel();

    function showUndoPanel()
    {
        var weekContainer = getDomOrCreateNew("MonthBar");
        weekContainer.appendChild(UndoPanelContainer);
        triggerTimer(NumberOfSeconds)
        $(UndoPanelContainer).removeClass("setAsDisplayNone");
        
        function triggerTimer(CountDownInSecs)
        {
            updateCountDown(CountDownInSecs);
            function updateCountDown(CurrentSec)
            {
                UndoCountDownTextContainer.innerHTML = "... "+CurrentSec + "s"
                if(CurrentSec>0)
                {
                    --CurrentSec;
                    setTimeout(function () { updateCountDown(CurrentSec) }, 1000);
                }
                else
                {
                    hideUndoPanel();
                }
            }
        }
    }

    

    function hideUndoPanel()
    {
        console.log("Hiding undo pannel")
        $(UndoPanelContainer).addClass("setAsDisplayNone");
        if (UndoPanelContainer.parentNode != null)
        {
            UndoPanelContainer.parentNode.removeChild(UndoPanelContainer);
        }
    }

    function clickUnDoPanel()
    {
        global_ExitManager.triggerLastExitAndPop();
        function undoCallBack() {
            hideUndoPanel();
            getRefreshedData.enableDataRefresh();
            getRefreshedData();
        }
        sendUndoRequest(undoCallBack);
        
    }
}


function configureAuthorizationToken(userName, password) {
    let url = window.location.origin + "/account/token";
    let LoginCredentials = {username: userName, password: password, "grant_type":'password'};
    let retValue = $.ajax({
        type: "POST",
        url: url,
        data: LoginCredentials,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        //dataType: "json",
        success: function (response) {
            debugger;
            let now = Date.now();
            let tokenLifeSpan = response['expires_in'];
            let tokenLifeSpanInMs = tokenLifeSpan * 1000
            let expiryTime = now + tokenLifeSpanInMs;
            let localStorage = window.localStorage;
            localStorage.setItem('bearerAccessToken', response['access_token']);
            localStorage.setItem('bearerExpiryDate', expiryTime);
            localStorage.setItem('bearerTokenType', response['token_type']);
        },
        error: function (err) {
            showRegistrationError(err);
            setTimeout(hideRegistrationError, 6000);
        }
    })

    return retValue
}

function updateSleepTimeline(sleepTimeline) {
    if(sleepTimeline) {
        global_sleepTimeline = [];
        let today = new Date(Date.now());
        let sleepStart = new Date(sleepTimeline.start);
        
        sleepStart.setDate(today.getDate());
        sleepStart.setFullYear(today.getFullYear());
        let sleepEnd = new Date(sleepStart.getTime() + sleepTimeline.duration);
        if(sleepStart.getDate() == sleepEnd.getDate()) {
            global_sleepTimeline.push({
                start: sleepStart,
                end: sleepEnd
            })
        } else {
            if(sleepStart.getTime() < sleepEnd.getTime()) {
                while(sleepStart.getDate() !== sleepEnd.getDate()) {
                    let nextSleepStart = new Date( sleepStart.getTime());
                    nextSleepStart.setDate((nextSleepStart.getDate()+1));
                    nextSleepStart.setHours(0,0,0,0);
                    let currentSleepEnd = new Date( nextSleepStart.getTime() - OneMinInMs);
                    global_sleepTimeline.push({
                        start: sleepStart,
                        end: currentSleepEnd
                    });
                    sleepStart = nextSleepStart;
                }

                global_sleepTimeline.push({
                    start: sleepStart,
                    end: sleepEnd
                });

            } else {
                throw "Sleep time line is invalid"
            }
        }
    }
}

function StructuralizeNewData(NewData)
{
    var TotalSubEventList = new Array();
    var ActiveSubEvents = new Array();
    var Dictionary_OfCalendarData = {};
    var Dictionary_OfSubEvents = {};
    if (NewData != "")
    {
        if (!NewData.Schedule.SubCalendarEvents) {
            generateNonRepeatEvents(NewData.Schedule.NonRepeatCalendarEvent);
            generateRepeatEvents(NewData.Schedule.RepeatCalendarEvent);
        } else {
            NewData.Schedule.SubCalendarEvents.forEach(SubCalendaEventsCreateDomElement)
        }

        updateSleepTimeline(NewData.Schedule.SleepTimeline);
        CleanupData();
    }
    else {
        console.log("Empty Data");
    }

    function generateRepeatEvents(AllRepeatSchedules) {
        AllRepeatSchedules.forEach(function (EachRepeatEvent) { EachRepeatEvent.RepeatCalendarEvents.forEach(CalendarCreateDomElement) });
    }

    function generateNonRepeatEvents(AllNonRepeatingEvents) {
        AllNonRepeatingEvents.forEach(CalendarCreateDomElement);
    }

    function SubCalendaEventsCreateDomElement(SubCalendarEvent) {
        //function is responsible for populating Dictionary_OfSubEvents. It also tries to populate the respective sub event dom
        var UIColor = {};
        UIColor.R = SubCalendarEvent.RColor;
        UIColor.G = SubCalendarEvent.GColor;
        UIColor.B = SubCalendarEvent.BColor;
        UIColor.O = SubCalendarEvent.OColor;
        UIColor.S = SubCalendarEvent.ColorSelection;
        let CalendarEvent = SubCalendarEvent.CalEvent;
        var CalendarData = { CompletedEvents: CalendarEvent.NumberOfCompletedTasks|| 0, DeletedEvents: CalendarEvent.NumberOfDeletedEvents||0, TotalNumberOfEvents: CalendarEvent.NumberOfSubEvents||0, UI: UIColor, Rigid: CalendarEvent.Rigid };
        Dictionary_OfCalendarData[CalendarEvent.ID] = CalendarData;
        SubCalendarEvent.Name = SubCalendarEvent.SubCalCalendarName
        TotalSubEventList.push(PopulateDomForScheduleEvent(SubCalendarEvent, CalendarEvent.Tiers, CalendarData));
    }


    function CleanupData()
    {
        TotalSubEventList.sort(function (a, b) { return (a.SubCalStartDate) - (b.SubCalStartDate) });

        
        var NowDate = new Date(CurrentTheme.Now);
        TotalSubEventList.forEach(
            function (eachSubEvent)
            {
                if (Dictionary_OfSubEvents[eachSubEvent.ID] == null) {
                    Dictionary_OfSubEvents[eachSubEvent.ID] = eachSubEvent;
                }
                else {
                    ToBeReorganized.push(eachSubEvent);

                }
                var RangeStart = new Date(NowDate.getTime() - (OneHourInMs * 12));
                var RangeEned = new Date(CurrentTheme.Now + TwelveHourMilliseconds);

            
            }
        )

    }




    function PopulateDomForScheduleEvent(myEvent, Tiers, CalendarData) {
        myEvent.Start = myEvent.SubCalStartDate;
        myEvent.End = myEvent.SubCalEndDate;
        myEvent.SubCalStartDate = new Date(myEvent.SubCalStartDate);// + global_TimeZone_ms);
        myEvent.SubCalEndDate = new Date(myEvent.SubCalEndDate);// + global_TimeZone_ms);
        myEvent.SubCalCalEventStart = new Date(myEvent.SubCalCalEventStart);//+ global_TimeZone_ms);
        myEvent.SubCalCalEventEnd = new Date(myEvent.SubCalCalEventEnd);// + global_TimeZone_ms);
        myEvent.Tiers = Tiers;
        myEvent.Split = CalendarData.TotalNumberOfEvents
        myEvent.DeletionCount = CalendarData.DeletedEvents
        myEvent.CompletionCount = CalendarData.CompletedEvents
        return myEvent
    }
    return { TotalSubEventList: TotalSubEventList,ActiveSubEvents: ActiveSubEvents,Dictionary_OfCalendarData: Dictionary_OfCalendarData,Dictionary_OfSubEvents:Dictionary_OfSubEvents};
}

function getEventsWithinRange(RangeStart, RangeEned, subEvents) {
    var RetValue = new Array();
    if (!subEvents) {
        subEvents = TotalSubEventList;
    }
    
    for (var i = 0 ; i < subEvents.length; i++) {
        var eachSubEvent = subEvents[i];
        if ((eachSubEvent.SubCalEndDate > RangeStart)  ) {
            if ((eachSubEvent.SubCalStartDate <= RangeEned))
            {
                RetValue.push(eachSubEvent);
            }
            else
            {
                break;
            }
        }
        
    }

    return RetValue;

    /*
    if ((eachSubEvent.SubCalEndDate > RangeStart) && (eachSubEvent.SubCalStartDate < RangeEned) && (RetValue.indexOf(eachSubEvent) < 0))
    {
        var Difference = eachSubEvent.SubCalEndDate.getTime() - NowDate.getTime();
        if (Difference < 0) {
            Difference *= -1;
        }

        if (Difference < lowestMsToNow) {
            ClosestSubEventToNow = eachSubEvent;
        }
        RetValue.push(eachSubEvent);
    }
    */
}

function getClosestToNow(AllEvents, ReferenceTime) {
    var RetValue;

    if (AllEvents.length) {
        RetValue = AllEvents[0];
        var lowestMsToNow = AllEvents[0].SubCalEndDate.getTime() - ReferenceTime.getTime();
        if (lowestMsToNow < 0) {
            lowestMsToNow *= -1;
        }
        for (var i = 0 ; i < AllEvents.length; i++) {
            var eachSubEvent = AllEvents[i];
            var Difference = eachSubEvent.SubCalEndDate.getTime() - ReferenceTime.getTime();
            if (Difference < 0) {
                Difference *= -1;
            }

            if (Difference < lowestMsToNow) {
                RetValue = eachSubEvent;
            }
        }
    }
    return RetValue;
}




function sendUndoRequest(CallBack)
{
    var TimeZone = new Date().getTimezoneOffset();
    var UndoData = {
        UserName: UserCredentials.UserName, UserID: UserCredentials.ID, TimeZoneOffset: TimeZone
    };
    UndoData.TimeZone = moment.tz.guess()
    var HandleNEwPage = new LoadingScreenControl("Tiler is undoing your last request :)");
    HandleNEwPage.Launch();

    var exitSendMessage = function (data) {
        HandleNEwPage.Hide();
    }
    
    //var URL= "RootWagTap/time.top?WagCommand=2";
    var URL = global_refTIlerUrl + "Schedule/Undo";
    var HandleNEwPage = new LoadingScreenControl("Tiler is Undoing :)");
    preSendRequestWithLocation(UndoData);
    HandleNEwPage.Launch();
    let undoRequest = $.ajax({
        type: "POST",
        url: URL,
        data: UndoData,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        dataType: "json",
        success: CallBackSuccess,
        error: CallBackFailure,
    }).done(() => {
        sendPostScheduleEditAnalysisUpdate({});
    })

    function CallBackSuccess()
    {
        HandleNEwPage.Hide();
        CallBack();
        return;
    }

    function CallBackFailure()
    {
        var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
        var ExitAfter = {
            ExitNow: true, Delay: 1000
        };
        HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exitSendMessage);
        return;
    }
}

function delete_cookie()
{
    var CookieName = "TilerCaluserWaggy"
    document.cookie = CookieName + '=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
}


function global_goToError()
{
    var urlString = "Error.html"
    window.location.href = urlString;
    return;

}

function global_goToLoginPage() {
    debugger
    var urlString = "Index.html"
    window.location.href = urlString;
    return;

}



function TestPostCall()
{
    var myLocation = new Location("Home", "1111 Oak Tree Ave Norman OK");

    var eventStart = { Time: { Hour: new Date(CurrentTheme.Now).getHours(), Minute: new Date(CurrentTheme.Now).getMinutes() }, Date: new Date(CurrentTheme.Now) };
    var CurrThemeEnd = new Date(CurrentTheme.Now + OneDayInMs);

    var eventEnd = { Time: { Hour: CurrThemeEnd.getHours(), Minute: CurrThemeEnd.getMinutes() }, Date: CurrThemeEnd };


    var DayPlusOne = new Date(CurrentTheme.Now);
    var Day = DayPlusOne.getDate();
    var Month = DayPlusOne.getMonth() + 1;
    var Year = DayPlusOne.getFullYear();
    var DatePickerValue = Month + "/" + Day + "/" + Year;


    var RepetitionStart = DatePickerValue;

    DayPlusOne = new Date(CurrentTheme.Now + OneWeekInMs);
    Day = DayPlusOne.getDate();
    Month = DayPlusOne.getMonth() + 1;
    Year = DayPlusOne.getFullYear();
    DatePickerValue = Month + "/" + Day + "/" + Year;



    var RepetitionEnd = DatePickerValue;


    var newEvent = new CalEventData("Jerome Event", myLocation, 1, new Color(140, 200, 140), { Days: 0, Hours: 1, Mins: 20 }, eventStart, eventEnd, "", RepetitionStart, RepetitionEnd, 1);
    
    var newEventString = JSON.stringify(newEvent);
    
    //url: "RootWagTap/time.top" + "?ac=send",
    
    var TimeZone = new Date().getTimezoneOffset(); 
    
    newEvent.TimeZoneOffset = TimeZone;
    $.ajax({
        type: "POST",
        url: "RootWagTap/time.top?WagCommand=1",
        data: newEvent,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        dataType: "json",
        success: function (response) {
            alert(response.d);
        }
    });
}


function getNewDividerDom() {
    var TopSlider = document.createElement("div");//populates this DOM
    $(TopSlider).addClass(CurrentTheme.Divider);//Add css class
    return TopSlider;
}





function getHomePageDomContainer()
{
    return getDomOrCreateNew("HomePageContainer");
}

function getNextMont(now)
{
    now = new Date(now.getFullYear(), now.getMonth(), 1);
    var retValue 
    if (now.getMonth() == 11) {
        retValue = new Date(now.getFullYear() + 1, 0, 1);
    } else {
        retValue = new Date(now.getFullYear(), now.getMonth() + 1, 1);
    }
    return retValue;
}

function OutOfFocusManager()
{
    var AllCallBackFunc = [];
    var CurrIndex = -1;
    function triggerLastExit()
    {
        if (CurrIndex >= 0)
        {
            if (!AllCallBackFunc[CurrIndex].isNotExitable)
            {
                AllCallBackFunc[CurrIndex]();
            }
        }
    }

    function triggerLastExitAndPop()
    {
        if (CurrIndex >= 0)
        {
            triggerLastExit();
            AllCallBackFunc.pop();
            --CurrIndex;
            AddCloseButoon.HideClosButton();
        }
    }

    function checkKeyBoardTrigger(e)
    {
        if (e.which == 27)
        {
            triggerLastExitAndPop();
        }
    }

    function addNewExit(NewCallBAckFunc)
    {
        AllCallBackFunc.push(NewCallBAckFunc);
        ++CurrIndex;
    }

    document.onkeydown = checkKeyBoardTrigger;
    //this.triggerLastExit = triggerLastExit;
    this.triggerLastExitAndPop = triggerLastExitAndPop;
    this.addNewExit = addNewExit;
}


function DateTimeToDayMDYTimeString(DateData)
{
    var DayString = WeekDays[DateData.getDay()];
    var MonthString = Months[DateData.getMonth()];
    var MonthDay = DateData.getDate();
    var YearData = DateData.getFullYear();
    var TimeString = getTimeStringFromDate(DateData);
    var retValue = DayString + ", " + MonthString + " " + MonthDay + " " + YearData + " " + TimeString;
    return retValue;
}

function RetrieveDom(DomObject_Or_ID) {
    var DomObject;
    if ((typeof (DomObject_Or_ID)).toUpperCase() == "STRING")
    {
        DomObject = document.getElementById(DomObject_Or_ID);
    }
    else {
        DomObject = DomObject_Or_ID;
    }
    if (DomObject == null) {
        alert("Couldnt Retrieve DomObject:\t" + DomObject_Or_ID)
    }
    return DomObject;
}

function InsertVerticalLine(PercentHeight, LeftPosition, TopPosition, thickness, Alternate) {
    if (thickness == null) {
        thickness = "4px";
    }

    var Line = document.createElement("div");//populates this DOM
    $(Line).addClass(CurrentTheme.LineColor);//Add css class
    if (Alternate === true) {
        $(Line).addClass(CurrentTheme.AlternateLineColor);//Add css class
    }


    Line.style.position = 'absolute';
    Line.style.width = thickness;
    Line.style.height = PercentHeight;
    Line.style.left = LeftPosition;
    Line.style.top = TopPosition;
    return Line;
}

function InsertHorizontalLine(PercentWidth, LeftPosition, TopPosition, thickness, Alternate) {
    if (thickness == null) {
        //thickness = "4px";
    }

    var Line = document.createElement("div");//populates this DOM
    $(Line).addClass(CurrentTheme.LineColor);//Add css class
    if (Alternate === true) {
        $(Line).addClass(CurrentTheme.AlternateLineColor);//Add css class
    }


    Line.style.position = 'absolute';
    Line.style.width = PercentWidth;
    Line.style.height = thickness;
    Line.style.left = LeftPosition;
    Line.style.top = TopPosition;
    return Line;
}

function generateuserInput(InputParams, JqueryCss)
{
    //var InputParams = { Name: "nskjfdnkf", ID: "fdjfdhjf", Default: "hfdjdhjks" };
    

    var FullLabel = getDomOrCreateNew(InputParams.ID);
    //(FullLabel.Dom).style.width = "100%";
    //(FullLabel.Dom).style.height = "100px";
    (FullLabel.Dom).style.position = "relative";
    //(FullLabel.Dom).style.display = "table";
    //(FullLabel.Dom).style.borderBottom = "3px solid rgb(127,127,127)";
    if (JqueryCss != null)
    {
        $(FullLabel.Dom).css(JqueryCss);
    }

    
    var LabelContainer = getDomOrCreateNew(InputParams.ID + "Label","label");
    var InputContainer = getDomOrCreateNew(InputParams.ID + "Input", "input");
    LabelContainer.Dom.innerHTML = InputParams.Name;
    LabelContainer.Dom.style.width = "20%";
    LabelContainer.Dom.style.left = "0%";
    LabelContainer.Dom.style.position = "absolute";

    InputContainer.Dom.style.width = "78%";
    InputContainer.Dom.style.left = "20%";
    InputContainer.Dom.style.height = "40px";
    //InputContainer.Dom.style.maxHeight = "40%";
    //InputContainer.Dom.style.fontSize = "12px";
    InputContainer.Dom.style.position = "relative";
    
    InputContainer.Dom.setAttribute("placeholder", InputParams.Default);


    $(FullLabel.Dom).append(LabelContainer.Dom);
    $(FullLabel.Dom).append(InputContainer.Dom);
    return { Input: InputContainer, Label:LabelContainer, FullContainer: FullLabel };
}
/*
*Utility functions End
*/





/*
*Classes
*/
function Theme(color)
{
    this.Color=color;
    this.ContentSection = "ContentSection" + this.Color;
    this.AlternateContentSection = "AlternateContentSection" + this.Color;
    this.AddButton="AddButton"+this.Color;
    this.DirectionsIcon="DirectionsIcon";
    this.SilentIcon="SilentIcon";
    this.ProcrastinateIcon = "ProcrastinateIcon";
    this.ProcrastinateAllIcon = "ProcrastinateAllIcon";
    this.CompleteIcon = "CompleteIcon";
    this.RigidEventPin="RigidEventPin";
    this.EventSection="EventSection"+this.Color;
    this.Divider = "Divider" + this.Color;
    this.DefaultEventColor = "DefaultEventBackGround" + this.Color;
    var CurrentContainer = null;
    var ContainerStack = new Array();
    this.Container = CurrentContainer;
    this.TransitionNewContainer = transitionNewContainer;
    this.TransitionOldContainer = transitionOldContainer;
    this.getCurrentContainer = GetCurrentContainer;
    this.AppUIContainer = document.getElementById("CalBodyContainer");
    this.Now = Date.now();
    this.FontColor = "TextColor" + this.Color;
    this.AlternateFontColor = "AlternateTextColor" + this.Color;
    this.BorderColor = "BorderColor" + this.Color;
    this.AlternateBorderColor = "AlternateBorderColor" + this.Color;
    this.LineColor = "EventLineColor" + this.Color;
    this.Border = "Border" + this.Color;
    this.AlternateBorder = "AlternateBorder" + this.Color;
    this.AlternateLineColor = "AlternateEventLineColor" + this.Color;
    this.ActiveTabTitle = "ActiveTabTitle" + this.Color;
    this.InActiveTabTitle = "InActiveTabTitle" + this.Color;
    this.ActiveTabContent = "ActiveTabContent" + this.Color;
    this.InActiveTabContent = "InActiveTabContent" + this.Color;
    this.RadialBackGround = "RadialBackGround" + this.Color;
    this.WagTapColor = "RadialBackGround" + this.Color;
    this.SearchIcon = "SearchIcon" + this.Color;
    this.LoadingImage = "LoadingImage" + this.Color;
    this.getMobileOS = getMobileOperatingSystem;


    function getMobileOperatingSystem() {
        var userAgent = navigator.userAgent || navigator.vendor || window.opera;

        if (userAgent.match(/iPad/i) || userAgent.match(/iPhone/i) || userAgent.match(/iPod/i)) {
            return 'iOS';

        }
        else if (userAgent.match(/Android/i)) {

            return 'Android';
        }
        else {
            return 'unknown';
        }
    }
    function transitionNewContainer(NewContainer)
    {
        HideCurrentContainer();
        updateCurrentContainer(NewContainer);
        LoadNewContainer();
        //document.getElementById("CalBodyContainer").appendChild(NewContainer);
    }

    function transitionOldContainer()
    {
        HideCurrentContainer(true);
        ContainerStack.pop();
        LoadNewContainer();
    }

    function updateCurrentContainer(NewContainer)
    {
        CurrentContainer = NewContainer;
        NewContainer.style.left = "100%";
        ContainerStack.push(CurrentContainer);
    }

    function LoadNewContainer()
    {
        CurrentContainer = GetCurrentContainer();
        if (CurrentContainer != null)
        {
            //CurrentContainer.style.left = "100%";
            document.getElementById("CalBodyContainer").appendChild(CurrentContainer);
            
            setTimeout(deferSlideInCall(CurrentContainer, "0%", "0%"), 1);
            
        }
    }

    function deferSlideInCall(Dom, top, left)
    {
        return function ()
        {
            if (left != null)
            { Dom.style.left = left }
            if (top!= null)
            { Dom.style.top = top }
        }
    }

    function getOldContainer()
    {
        var ContainerLength=ContainerStack.length();
        if ((ContainerLength) > 0)
        {
            return ContainerStack[ContainerLength - 1];
        }
        else
        {
            return null;
        }
    }

    function HideCurrentContainer(SlideRight)
    {
        CurrentContainer = GetCurrentContainer();
        /*
        if (CurrentContainer != null)
        {
            if (SlideRight===true)
            {
                CurrentContainer.style.left = "100%";
            }
            else
            {
                CurrentContainer.style.left = "-100%";
            }
        }
        */
    }

    function GetCurrentContainer()
    {
        var ContainerLength = ContainerStack.length;
        if ((ContainerLength) > 0) {
            return ContainerStack[ContainerLength - 1];
        }
        else {
            return null;
        }
    }
}

function formatTimePortionOfStringToRightFormat(TimeString)
{

    //"jkjkjkj".
    TimeString = TimeString.toLocaleLowerCase().split("");
    var IndexOfA = TimeString.indexOf("a");
    var IndexOfB = TimeString.indexOf("p");
    var myIndex = IndexOfA > IndexOfB ? IndexOfA : IndexOfB;
    var TimeCOncat = [];
    var retValue = "";
    var i = 0;
    for (; i < myIndex; i++)
    {
        if(TimeString[i]!=" ") {
            retValue+=TimeString[i]
        }
    }
    retValue += (" " + TimeString[i] + "m");
    return retValue;

}

function getTimeStringFromDate(date)
{
    
    var myString = new Date(date).toLocaleString();
    //var TimeZoneOffset=new Date().getTimezoneOffset()/60;
    var hours = date.getHours();
    //hours += TimeZoneOffset;
    //hours %= 24;
    var minutes = date.getMinutes();
    var ampm = hours >= 12 ? 'pm' : 'am';
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var strTime = hours + ':' + minutes + ' ' + ampm;
    return strTime;
}


function getHourMin(date)
{
    var TimeZoneOffset = new Date().getTimezoneOffset() / 60;
    var hours = date.getHours();
    hours += TimeZoneOffset;
    hours %= 24;
    var minutes = date.getMinutes();
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var retValue = { Hour: hours, Minute: minutes };
    return retValue;
}


function procrastinateSubEvent(ID, Day, Hour, Minute,CallBackSuccess,CallBackFailure,CallBackDone)
{
    var HourInput = Hour
    var MinInput = Minute
    var DayInput = Day;
    var TimeZone = new Date().getTimezoneOffset();
    var NowData = {
        UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: ID, DurationDays: DayInput, DurationHours: HourInput, DurationMins: MinInput, TimeZoneOffset: TimeZone
    };
    //var URL= "RootWagTap/time.top?WagCommand=2";
    var URL = global_refTIlerUrl + "Schedule/Event/Procrastinate";
    var HandleNEwPage = new LoadingScreenControl("Tiler is Postponing  :)");
    NowData.TimeZone = moment.tz.guess()
    preSendRequestWithLocation(NowData);
    HandleNEwPage.Launch();
    var ProcrastinateRequest = $.ajax({
        type: "POST",
        url: URL,
        data: NowData,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        dataType: "json",
        success: CallBackSuccess,
        error:CallBackFailure,
    })

    if(CallBackDone!=undefined)
    {
        ProcrastinateRequest.done(CallBackDone);
    }
}

function sendPostScheduleEditAnalysisUpdate({CallBackSuccess, CallBackFailure, CallBackDone}) {
    let TimeZone = new Date().getTimezoneOffset();
    let postData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, TimeZoneOffset: TimeZone };
    let url = global_refTIlerUrl + "Analysis/Analyze";
    preSendRequestWithLocation(postData);

    let scheduleAnalysisRequest= $.ajax({
        type: "POST",
        url: url,
        data: postData,
        dataType: "json",
        success: CallBackSuccess,
        error: CallBackFailure
    })
    
    scheduleAnalysisRequest.done((response) => {
        getRefreshedData()
        if (isFunction(CallBackDone)) {
            CallBackDone(response)
        }
    })
    
}

function setSubCalendarEventAsNow(SubEventID, CallBackSuccess, CallBackFailure, CallBackDone)
{
    var TimeZone = new Date().getTimezoneOffset();
    var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: SubEventID, TimeZoneOffset: TimeZone };
    var URL = global_refTIlerUrl + "Schedule/Event/Now";
    preSendRequestWithLocation(NowData);

    var ProcrastinateRequest= $.ajax({
        type: "POST",
        url: URL,
        data: NowData,
        dataType: "json",
        success: CallBackSuccess,
        error: CallBackFailure
    })
    ProcrastinateRequest.done(() => {
        if (CallBackDone != undefined) {
            CallBackDone()
        }
        sendPostScheduleEditAnalysisUpdate({})        
    });    
}

function SetCalendarEventAsNow(CalendarEventID, CallBackSuccess, CallBackFailure, CallBackDone) {
    var TimeZone = new Date().getTimezoneOffset();
    var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, ID: SubEventID, TimeZoneOffset: TimeZone };
    var URL = global_refTIlerUrl + "CalendarEvent/Now/";
    preSendRequestWithLocation(NowData);

    var AjaxRequest = $.ajax({
        type: "POST",
        url: URL,
        data: NowData,
        dataType: "json",
        success: CallBackSuccess,
        error: CallBackFailure
    })

    AjaxRequest.done(() => {
        if (CallBackDone != undefined) {
            CallBackDone()
        }
        sendPostScheduleEditAnalysisUpdate({})        
    });
}

function deleteSubEvent(SubEventID, CallBackSuccess, CallBackFailure, CallBackDone)
{
    var TimeZone = new Date().getTimezoneOffset();
    var DeletionEvent = {
        UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: SubEventID, TimeZoneOffset: TimeZone
    };
    //var URL = "RootWagTap/time.top?WagCommand=6"
    var URL = global_refTIlerUrl + "Schedule/Event";
    var HandleNEwPage = new LoadingScreenControl("Tiler is Deleting your event :)");
    DeletionEvent.TimeZone = moment.tz.guess()
    preSendRequestWithLocation(DeletionEvent);
    HandleNEwPage.Launch();

    var AjaxRequest = $.ajax({
        type: "DELETE",
        url: URL,
        data: DeletionEvent,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        success: CallBackSuccess,
        error: CallBackFailure
    });

    AjaxRequest.done(() => {
        if (isFunction(CallBackDone)) {
            CallBackDone()
        }
        sendPostScheduleEditAnalysisUpdate({})        
    });
}


function deleteCalendarEvent(CalendarEventID, CallBackSuccess, CallBackFailure, CallBackDone) {
    var TimeZone = new Date().getTimezoneOffset();
    var DeletionEvent = {
        UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: CalendarEventID, TimeZoneOffset: TimeZone
    };
    //var URL = "RootWagTap/time.top?WagCommand=6"
    var URL = global_refTIlerUrl + "CalendarEvent";
    var HandleNEwPage = new LoadingScreenControl("Tiler is Deleting your event :)");
    HandleNEwPage.Launch();
    DeletionEvent.TimeZone = moment.tz.guess()
    preSendRequestWithLocation(DeletionEvent);
    var AjaxRequest = $.ajax({
        type: "DELETE",
        url: URL,
        data: DeletionEvent,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        success: CallBackSuccess,
        error: CallBackFailure
    });

    AjaxRequest.done(() => {
        if (isFunction(CallBackDone)) {
            CallBackDone()
        }
        sendPostScheduleEditAnalysisUpdate({})        
    });
}

function completeCalendarEvent(CalendarEventID, CallBackSuccess, CallBackFailure, CallBackDone)
{

    var TimeZone = new Date().getTimezoneOffset();
    var Url;
    //Url="RootWagTap/time.top?WagCommand=7";
    Url = global_refTIlerUrl + "CalendarEvent/Complete";
    var HandleNEwPage = new LoadingScreenControl("Tiler is updating your schedule ...");
    HandleNEwPage.Launch();

    var MarkAsCompleteData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: CalendarEventID, TimeZoneOffset: TimeZone };
    MarkAsCompleteData.TimeZone = moment.tz.guess()
    preSendRequestWithLocation(MarkAsCompleteData);
    var AjaxRequest = $.ajax({
        type: "POST",
        url: Url,
        data: MarkAsCompleteData,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        //dataType: "json",
        success: CallBackSuccess,
        error: CallBackFailure
    });

    AjaxRequest.done(() => {
        if (isFunction(CallBackDone)) {
            CallBackDone()
        }
        sendPostScheduleEditAnalysisUpdate({})        
    });
}

    function createEvent(EventEntry)
    {
        var DivOfEvent = document.getElementById(EventEntry.ID);
        if (EventEntry.WeekActivity == null)
        {
            alert("invalid schedule");
        }
        alert(EventEntry.WeekActivity.status);

        if ((DivOfEvent == null)||(DivOfEvent == undefined))
        {
            DivOfEvent = document.createElement("div");
            DivOfEvent.id = EventEntry.ID
            DivOfEvent.style.position = "absolute";
            DivOfEvent.setAttribute("class", EventEntry.ID);
            $(DivOfEvent).addClass("CalEvent");
        }
        var Duration = EventEntry.Duration;
        var DivHeightPixels = milliSecondsToPixels(Duration);
        var TestDate = EventEntry.StartTime;

        var DayOfEvent = TestDate.getDate();
        var YearOfEvent = TestDate.getFullYear();
        var MonthOfEvent = TestDate.getMonth();
        var TopDate = new Date(YearOfEvent, MonthOfEvent, DayOfEvent, 0, 0, 0, 0);
        var TopMargin = TestDate - TopDate;
        TopMargin = milliSecondsToPixels(TopMargin);
        TopMargin = Math.round(TopMargin);
        DivOfEvent.style.top = TopMargin + "px";
        DivOfEvent.style.left = (EventEntry.WeekActivity.weekIndex*100)+"px"
        DivOfEvent.style.width = (WidthOfDay - 10) + "px";
        DivOfEvent.style.height = DivHeightPixels + "px";
        var DivForUpdate= new Array();
        DivForUpdate.push(DivOfEvent);
        return DivOfEvent
    }

    function milliSecondsToPixels(milliSeconds)
    {
        var millisecondsPerHour = 3600 * 1000;
        var Hours = milliSeconds / millisecondsPerHour;
        return HeightPerHour * Hours;
    }

    function MyEvent(Name, Starttime, Endtime, Location,ID)
    {
        this.Name = Name;
        this.StartTime = Starttime;
        this.EndTime = Endtime;
        this.ID = ID;
        this.Duration = this.EndTime - this.StartTime;
        this.WeekActivity = isEventActive(this);
    }

    function isEventActive(Event)
    {
        var RetValue = new Array();
        var NumberOfDaysInAweek = 7;
        var CurrentDay = { start: new Date(), end: new Date() };
        var i = 0;
        for (i = 0; i < NumberOfDaysInAweek; i++)
        {
            CurrentDay.start = new Date(CurrentFullSchedule[i].getTime());
            CurrentDay.end = new Date(CurrentFullSchedule[i].getTime() + (i * 24 * 60 * 60 * 1000));
            if ((Event.StartTime >= CurrentDay.start) && (Event.EndTime <= CurrentDay.end)) {
                return { status: 1, day: CurrentDay,weekIndex:i };
            }
            else
            {
                if(((Event.StartTime <= CurrentDay.end) && (Event.EndTime >= CurrentDay.end))||((Event.Endtime >= CurrentDay.start) && (Event.StartTime < CurrentDay.start)))
                    return { status: 0, day: CurrentDay, weekIndex: i };
            }
        }

        return null;
    }
    function UpdateAllDayCalendarEvent(increment)
    {
        var NumberOfDaysInAweek = 7;
        var i = 0;
    
        for (i = 0; i < NumberOfDaysInAweek; i++)
        {
            CurrentFullSchedule[i] = [new Date(CurrentStartOfWeek.getTime() + (i * increment * 24 * 60 * 60 * 1000))];
        }
    }

    function calEvent(name, startTime,startDate, endTime,endDate, locationData, color, rigid, range, repetitionData, recurringDays)
    {
        this.Name = name;
        this.StartDateTime = startTime+startDate;
        this.EndDateTime = endTime + endDate;
        this.LocationData = locationData;
        this.Color = Color;
        this.Rigid = rigid;
        this.RepetitionData = repetitionData;
        this.RecurringDays = recurringDays;
    }



    function DialOnEvent(ParentDom,CallBackFunction,DefaultDial) {
    
        var EventDialButtonContainer;// = document.getElementById("EventDialButtonContainer" + DialOnEvent.id);
        ++DialOnEvent.id;
        ParentDom = CurrentTheme.AppUIContainer

        var EventDialContainer = getDomOrCreateNew("EventDialContainerSelected" + DialOnEvent.id);

        $(EventDialContainer.Dom).addClass("DialContainer");
        $(EventDialContainer.Dom).addClass(CurrentTheme.ContentSection);

        EventDialButtonContainer = document.createElement("div");
        EventDialButtonContainer.setAttribute("id", "EventDialButtonContainerSelected" + DialOnEvent.id);
        $(EventDialButtonContainer).addClass("SelectedDialButtonContainer");

        EventDialContainer.Dom.appendChild(EventDialButtonContainer);


        //$(EventDialButtonContainer).css({ "left": "0%", "height": "10%", "width": "100%", "z-index": "10" });

        var EventBackButton = document.createElement("div");
        var EventUpdateButton = document.createElement("div");
        var ActiveEventProcratination = document.createElement("div");

        EventDialButtonContainer.appendChild(EventBackButton);
        $(EventBackButton).click(function () {
            var myEventDialButtonContainer = EventDialContainer.Dom;
            //alert("empty....");
            //myEventDialButtonContainer.innerHTML = "";
            $(myEventDialButtonContainer).empty();
            myEventDialButtonContainer.outerHTML = "";
            CurrentTheme.TransitionOldContainer();
        });

        $(EventUpdateButton).click(function () {
            var myEventDialButtonContainer = EventDialContainer.Dom;
            //var CallBackValue = {Selection}


            $(myEventDialButtonContainer).empty();
            myEventDialButtonContainer.outerHTML = "";
            CurrentTheme.TransitionOldContainer();
            CallBackFunction(CurrentDial);
        });

        var DialInputContainer = getDomOrCreateNew("DialInputContainer" + DialOnEvent.id)
        $(DialInputContainer.Dom).addClass("DialInputContainer");

        EventDialButtonContainer.appendChild(EventUpdateButton);
        EventDialButtonContainer.appendChild(ActiveEventProcratination);


        $(EventDialButtonContainer).addClass(CurrentTheme.DefaultEventColor);

        $(EventBackButton).addClass("BackIcon");
        $(EventBackButton).css({ "position": "absolute", "left": "1.5%", "width": "50px", "height": "50px", "top": "50%", "margin-top": "-25px" });

        $(EventUpdateButton).addClass("CheckIcon");
        $(EventUpdateButton).css({ "position": "absolute", "left": "98.5%", "width": "50px", "height": "50px", "margin-left": "-50px", "top": "50%", "margin-top": "-25px" });



        $(ActiveEventProcratination).addClass("ActiveEventProcrastination");
        var CurrentDial, SelectedDialOptionDomButton;

        var DayTextBox = getDomOrCreateNew("DialDayInput" + DialOnEvent.id,"button");
        DayTextBox.Dom.innerHTML = "Day(s)";
        DayTextBox.Dom.setAttribute("class", "DialInput DialDayInput");
        $(DayTextBox.Dom).click(function () {
            var CurrentDayValue = new Dial(0, 1, 1, 0);
            CurrentDial = CurrentDayValue;
            $(SelectedDialOptionDomButton.Dom).removeClass("SelectedDialButton");
            SelectedDialOptionDomButton = DayTextBox;

            $(SelectedDialOptionDomButton.Dom).addClass("SelectedDialButton");
            InitializeDialDial(CurrentDayValue, EventDialContainer.Dom);
        })


        var HourTextBox = getDomOrCreateNew("DialHourInput" + DialOnEvent.id, "button");
        HourTextBox.Dom.setAttribute("class", "DialInput DialHourInput");
        HourTextBox.Dom.innerHTML = "Hour(s)";
        $(HourTextBox.Dom).click(function () {
            var CurrentHourValue = new Dial(0, 5, 0, 60 * 2);
            CurrentDial = CurrentHourValue;
            $(SelectedDialOptionDomButton.Dom).removeClass("SelectedDialButton");
            SelectedDialOptionDomButton = HourTextBox;
            $(SelectedDialOptionDomButton.Dom).addClass("SelectedDialButton");
            InitializeDialDial(CurrentHourValue, EventDialContainer.Dom);
        })

        var MinTextBox = getDomOrCreateNew("DialMinInput" + DialOnEvent.id, "button");
        MinTextBox.Dom.setAttribute("class", "DialInput DialMinInput");
        MinTextBox.Dom.innerHTML = "Min(s)";



        function InitializeDialDial(SelectedDial, ParentContainer) {
            var KnobHolder = getDomOrCreateNew("KnobContainer" + DialOnEvent.id);
            $(KnobHolder.Dom).empty();
            //KnobHolder.Dom.appendChild(getDomOrCreateNew("KnobJS0").Dom)
            var KnobJSCurrContainer = getDomOrCreateNew("KnobJSCurrContainer");
            $(KnobJSCurrContainer.Dom).addClass(CurrentTheme.FontColor);
            var KnobJSTimeUnitContainer = getDomOrCreateNew("KnobJSTimeUnitContainer");
            KnobJSCurrContainer.Dom.appendChild(KnobJSTimeUnitContainer.Dom);

            //var KnobJSCurrContainerHour = getDomOrCreateNew("KnobJSCurrContainerHour");
            //var KnobJSCurrContainerHour = getDomOrCreateNew("KnobJSCurrContainerHour");
            var KnobJSCurrDialNameContainer = getDomOrCreateNew("KnobJSCurrDialNameContainer");
            KnobJSCurrContainer.Dom.appendChild(KnobJSCurrDialNameContainer.Dom);
            var KnobJSCurrDialel0Container = getDomOrCreateNew("KnobJSCurrDialel0Container")
            var KnobJSCurrDialel1Container = getDomOrCreateNew("KnobJSCurrDialel1Container")
            KnobJSCurrDialNameContainer.Dom.appendChild(KnobJSCurrDialel0Container.Dom);
            KnobJSCurrDialNameContainer.Dom.appendChild(KnobJSCurrDialel1Container.Dom);


            //KnobJSCurrContainer.Dom.appendChild(KnobJSCurrContainerHour.Dom);
            //KnobJSCurrContainer.Dom.appendChild(KnobJSCurrContainerMin.Dom);


            KnobHolder.Dom.appendChild(KnobJSCurrContainer.Dom);

            $(KnobHolder.Dom).addClass("KnobContainer");

            var KnobJS = getDomOrCreateNew("KnobJS" + DialOnEvent.id);
            $(KnobJS.Dom).addClass("KnobJS");
            $(KnobJS.Dom).addClass("yui3-skin-sam");
            KnobJS.Dom.setAttribute("data-bgColor", "rgb(80,80,80)");
            //KnobJS.Dom.setAttribute("data-displayInput", "true");

            //$(KnobHolder.Dom).addClass(CurrentTheme.ContentSection);
            var CircleDialCOntainer = getDomOrCreateNew("CircleDialCOntainer")
            CircleDialCOntainer.Dom.appendChild(KnobJS.Dom);
            //KnobHolder.Dom.appendChild()
            KnobHolder.Dom.appendChild(CircleDialCOntainer.Dom);
            ParentContainer.appendChild(KnobHolder.Dom);

            YUI().use('dial', function (Y) {
                var v, up = 0, down = 0, i = 0
                        , $idir = $("#KnobJS0")
                        , $ival = $("#KnobJSTimeUnitContainer")
                        , $iEl0NameTU = $("#KnobJSCurrDialel0Container")
                        , $iEl1NameTU = $("#KnobJSCurrDialel1Container")
                        //, $imin = $("#KnobJSCurrContainerMin")
                        , incr = function (data) {
                            //alert("jay+");
                            SelectedDial.IncrementTotalTime(data);
                            $ival.html(SelectedDial.getel0() + ":" + SelectedDial.getel1())
                        }
                        , decr = function (data) {
                            //alert("jay-");
                            SelectedDial.DecrementTotalTime(data);
                            $ival.html(SelectedDial.getel0() + ":" + SelectedDial.getel1())
                        };

                KnobJSCurrDialel0Container.Dom.innerHTML = (SelectedDial.el0String());
                KnobJSCurrDialel1Container.Dom.innerHTML = (SelectedDial.el1String());
                var changeInValueFunc= function(meDial)
                {
                    incr(meDial.newVal);
                }

                var mydial = new Y.Dial({
                    min: 0,
                    max: 100000,
                    stepsPerRevolution: SelectedDial.getFullRevolution(),
                    value: 0,
                    diameter: 300,
                    minorStep: SelectedDial.getStep(),
                    majorStep: SelectedDial.getStep(),
                    decimalPlaces: 0,
                    strings: { label: 'Altitude in Kilometers:', resetStr: 'Reset', tooltipHandle: 'Drag to set' },
                    // construction-time event subscription
                    after: {
                        valueChange: Y.bind(changeInValueFunc, mydial)
                    }
                });
                mydial.render(KnobJS.Dom);
            });
        


            /*
            (function () {
                var v, up = 0, down = 0, i = 0
                        , $idir = $("#KnobJS0")
                        , $ival = $("#KnobJSTimeUnitContainer")
                        , $iEl0NameTU = $("#KnobJSCurrDialel0Container")
                        , $iEl1NameTU = $("#KnobJSCurrDialel1Container")
                        //, $imin = $("#KnobJSCurrContainerMin")
                        , incr = function () {
                            //alert("jay+");
                            SelectedDial.IncrementTotalTime();
                            $ival.html(SelectedDial.getel0() + ":" + SelectedDial.getel1())
                        }
                        , decr = function () {
                            //alert("jay-");
                            SelectedDial.DecrementTotalTime();
                            $ival.html(SelectedDial.getel0() + ":" + SelectedDial.getel1())
                        };
                KnobJSCurrDialel0Container.Dom.innerHTML = (SelectedDial.el0String());
                KnobJSCurrDialel1Container.Dom.innerHTML = (SelectedDial.el1String());
                //alert(KnobJSCurrDialel1Container.Dom.innerHTML);
                $(KnobJS.Dom).knob(
                                    {
                                        min: 0
                                    , max: SelectedDial.Max
                                    , displayInput: false
                                    , step: SelectedDial.Step
                                    , stopper: true
                                    , width: "100%"
                                        //, cursor:true
                                    , change: function () {
                                            if (v > this.cv) {
                                                if (up) {
                                                    decr();
                                                    up = 0;
                                                } else { up = 1; down = 0; }
                                            }
                                            else {
                                                if (v < this.cv) {
                                                    if (down) {
                                                        incr();
                                                        down = 0;
                                                    } else { down = 1; up = 0; }
                                                }
                                            }
                                            v = this.cv;
                                        }
                                    });
            })();
            */
        }


        DialInputContainer.Dom.appendChild(DayTextBox.Dom);
        DialInputContainer.Dom.appendChild(HourTextBox.Dom);
        DialInputContainer.Dom.appendChild(MinTextBox.Dom);
        ActiveEventProcratination.appendChild(DialInputContainer.Dom);
        CurrentDial = DefaultDial;
        SelectedDialOptionDomButton = HourTextBox;
        if (CurrentDial == null)

        { CurrentDial = new Dial(0, 5, 0, 60 * 2); SelectedDialOptionDomButton = HourTextBox; }

        $(SelectedDialOptionDomButton.Dom).addClass("SelectedDialButton");

        CurrentTheme.TransitionNewContainer(EventDialContainer.Dom);

        InitializeDialDial(CurrentDial, EventDialContainer.Dom);



        /*$(ParentDom).children().hide();
        $(ParentDom).append(EventDialContainer.Dom);*/
    }

    DialOnEvent.id = 0;

    //function Dial(TotalTimeUnit, step, base, el0String, el1String, max, DialType)
    function Dial(TotalTimeUnit, step,DialType)
    {
        var TotTime = new Object();
        var el0String, el1String;

        this.el0String = function () {
            return el0String;
        }
        this.el1String = function ()
        {
            return el1String;
        }
    
    

        var base = 60;
        var max = 60*2;;

        function UpdateDialType(DialTypeEntry)
        {
            switch (DialTypeEntry) {
                case 0:
                    {
                        el0String = "Hour(s)";
                        el1String = "Min(s)";
                        base = 60;
                        max = base * 2;
                        DialType = DialTypeEntry;
                    }
                    break;
                case 1:
                    {
                        el0String = "Day(s)";
                        el1String = "Hour(s)";
                        base = 24;
                        max = base * 2;
                        DialType = DialTypeEntry;
                    }
                    break;
                default:
                    {
                        el0String = "Hour(s)";
                        el1String = "Min(s)";
                        base = 60;
                        max = base * 2;
                        DialType = 0;
                    }
                    break;
            }
        }
        UpdateDialType(DialType);

        TotTime.TotalTimeUnit = TotalTimeUnit;
        this.Step = step;
        this.Max = max;
        this.Type = function () { return DialType};


        this.getel0 = getel0;

        function getel0() {
            return parseInt(TotTime.TotalTimeUnit / base);
        }

        this.getel1 = getel1
    
        function getel1() {
            return parseInt(TotTime.TotalTimeUnit % base);
        }

        this.getTotalTimeUnit =getTotalTimeUnit;

        function getTotalTimeUnit() {
            return TotTime.TotalTimeUnit;
        }

        this.UpdateTotalTime =UpdateTotalTime;

        function UpdateTotalTime(TotalTime) {
            TotTime.TotalTimeUnit = TotalTime;
        }

        this.IncrementTotalTime = f_IncrementTotalTime;
        function f_IncrementTotalTime(timeValue)
        {
            if (timeValue != null) {
                TotTime.TotalTimeUnit = timeValue
            }
            else
            {
                TotTime.TotalTimeUnit += step;
            }

        }

        this.getFullRevolution = function ()
        {
            return base;
        }

        this.getStep = function () {
            return step;
        }

        this.DecrementTotalTime = f_DecrementTotalTime;
        function f_DecrementTotalTime(timeValue)
        {
            if (timeValue != null)
            {
                TotTime.TotalTimeUnit = timeValue
            }
            else {
                TotTime.TotalTimeUnit += step;
            }
            TotTime.TotalTimeUnit -= step;
            if (TotTime.TotalTimeUnit < 0) {
                TotTime.TotalTimeUnit = 0;
            }
        }

        this.getTimeString = function ()
        {
            return getel0() + el0String+ " " + getel1() + el1String;
        }

        this.ToTimeSpan = function ()
        {
            switch (DialType)
            {
                case 0:
                    {
                    
                        var Hours = parseInt(TotTime.TotalTimeUnit / base);
                        var Mins = parseInt(TotTime.TotalTimeUnit % base);
                        var Days = parseInt(Hours / 24);
                        Hours= parseInt(Hours % 24);
                        return { Days: Days, Hours: Hours, Mins: Mins };
                    }
                    break;
                case 1:
                    {
                        var Days = parseInt(TotTime.TotalTimeUnit / base);
                        var Hours = parseInt(TotTime.TotalTimeUnit % base);
                        return { Days: Days, Hours: Hours, Mins: Mins };
                    
                    }
                    break;
                default:
                    return null;
            }
        }

    }


function Location(Tag, Address, LocationIsVerified, LocationId)
    {
        this.Tag = Tag;
        this.Address = Address;
        this.LocationIsVerified = LocationIsVerified;
        this.LocationId = LocationId;
    }

    function CalEventData(eventName, eventLocation, eventCounts, eventColor, eventDuration, eventStart, eventEnd, eventRepeatData, eventRepeatStart, eventRepeatEnd, rigidFlag, RestrictionData)
    {
        this.Name = eventName;
        this.LocationTag = eventLocation.Tag;
        this.LocationAddress = eventLocation.Address;
        this.LocationIsVerified = eventLocation.LocationIsVerified;
        this.LocationId = eventLocation.LocationId;
        this.RColor = eventColor.r;
        this.GColor = eventColor.g;
        this.BColor = eventColor.b;
        this.ColorSelection = null == eventColor.s ? 0 : eventColor.s;
        this.Opacity = null == eventColor.o ? 1 : eventColor.o;
        this.DurationDays = 0;// eventDuration.Days;
        this.DurationHours = eventDuration.Hours;
        this.DurationMins = eventDuration.Mins;
        this.StartHour = eventStart.IsDefault && !rigidFlag ? 0 : eventStart.Time.Hour;
        this.EndHour = eventEnd.Time.Hour;
        this.StartMins = eventStart.IsDefault && !rigidFlag ? 0 : eventStart.Time.Minute;
        this.EndMins = eventEnd.Time.Minute;
        this.StartDay = eventStart.Date.getDate();
        this.EndDay = eventEnd.Date.getDate();
    
        this.StartMonth = eventStart.Date.getMonth()+1
        this.EndMonth = eventEnd.Date.getMonth()+1;

        this.StartYear = eventStart.Date.getFullYear();
        this.EndYear = eventEnd.Date.getFullYear();

        this.Rigid = rigidFlag ? true : false;

        var isRestricted = false;
        var RestrictionStart = "12:00am"
        var RestrictionEnd = "12:00am"
        var isWorkWeek = false;
        var isEveryDay = false;
        var RestrictiveWeek = null

        if ((RestrictionData != null)&&(!rigidFlag))
        {
            if (RestrictionData.isRestriction)
            {
                isRestricted = true;
                RestrictionStart = RestrictionData.Start;
                RestrictionEnd = RestrictionData.End;
                isWorkWeek = RestrictionData.isWorkWeek;
                isEveryDay = RestrictionData.isEveryDay;
            }
            RestrictiveWeek = RestrictionData.RestrictiveWeek;
        }


        this.isRestricted = isRestricted;
        this.RestrictionStart = RestrictionStart;
        this.RestrictionEnd = RestrictionEnd;
        this.isWorkWeek = isWorkWeek;
        this.isEveryDay = isEveryDay;
        this.RestrictiveWeek = RestrictiveWeek;

        this.RepeatData = eventRepeatData;
        var RepeatType = "";
        this.RepeatType = "";
        var RepeatFrequency = "";
        var Days0fWeek="";
        if (eventRepeatData.Type != null)
        {
       
            //if (eventRepeatData.Type.Index)
            {
                RepeatType = eventRepeatData.Type.Index;
                RepeatFrequency = eventRepeatData.Type.Name;
                if (eventRepeatData.Type.Index == 1)
                {
                    eventRepeatData.Misc.AllDoms.forEach(function (eachDom) { if (eachDom.status) { Days0fWeek += "" + eachDom.DayOfWeekIndex + "," } });
                }
                if (this.isEveryDay === undefined) {
                    if (RepeatFrequency.toLowerCase() === 'daily') {
                        this.isEveryDay = true
                    }
                }
            }
        }
        this.isEveryDay = this.isEveryDay || false
        this.RepeatType = RepeatType;
        this.RepeatWeeklyData = Days0fWeek;
        this.RepeatFrequency = RepeatFrequency;


    
        var repeatStartDate = date_mm_dd__yyyy_ToDateObj(( eventRepeatStart),"/");
        this.RepeatStartDay = repeatStartDate.getDate();
        this.RepeatStartMonth = repeatStartDate.getMonth() + 1;
        this.RepeatStartYear = repeatStartDate.getFullYear();


        var repeatEndDate = date_mm_dd__yyyy_ToDateObj((eventRepeatEnd), "/");
        this.RepeatEndDay = repeatEndDate.getDate();
        this.RepeatEndMonth = repeatEndDate.getMonth() + 1;
        this.RepeatEndYear = repeatEndDate.getFullYear();
        
        //alert(rigidFlag);
        this.Count = eventCounts;
    }


    function getTotalDurationFromCalEvent(CalEvent)
    {
        var TotalDurationInMs = (CalEvent.DurationDays * OneDayInMs) + (CalEvent.DurationHours * OneHourInMs) + (CalEvent.DurationMins * OneHourInMs);
        return TotalDurationInMs;
    }

    function getCalEventEnd(CalEvent)
    {
        //CalEvent = new CalEventData();
        var RetValue = new Date(CalEvent.EndYear,CalEvent.EndMonth-1,CalEvent.EndDay,0,0,0,0)
        return RetValue;
    }

    /*Function tries to check if the passed object (d) is a valid date object*/
    function isDateValid(d)
    {
        var RetValue = true;
        if (Object.prototype.toString.call(d) === "[object Date]")
        {
            if (isNaN(d.getTime())) {  // d.valueOf() could also work
                // date is not valid
                RetValue = false;
            }
            else {
                // date is valid
                RetValue = true;
            }
        }
        else {
            // not a date
            RetValue = false;
        }

        return RetValue;
    }

    function date_mm_dd__yyyy_ToDateObj(DateString,Delimiter)
    {
        var DateArray = DateString.split(Delimiter);
    
        var retValue = new Date(DateArray[2], DateArray[0]-1, DateArray[1], 0, 0, 0, 0);
        return retValue;
    }

    function getDateString(dateObj)
    {
        var retValue = "";
        if (dateObj != null)
        {
            retValue = (dateObj.getMonth() + 1) + "/" + dateObj.getDate() + "/" + dateObj.getFullYear();
        }
        return retValue;
    }

    function AP_To24Hour(time_str)
    {
        // Convert a string like 10:05:23 PM to 24h format, returns like [22,5,23]
        time_str=time_str.split(" ").join('')
        var time = time_str.match(/(\d+):(\d+)(\w)/);
        var hours = Number(time[1]);
        var minutes = Number(time[2]);
        var seconds = 0;//Number(time[3]);
        var meridian = time[3].toLowerCase();

        if (meridian == 'p' && hours < 12) {
            hours = hours + 12;
        }
        else if (meridian == 'a' && hours == 12) {
            hours = hours - 12;
        }

        var retValue = { Hour: hours, Minute: minutes };
        //var retValue={ Hour: sHours, Minute: sMinutes };
        return retValue;
    }

    function Color(r,g,b)
    {
        this.r=r;
        this.g=g
        this.b = b;

    }

    function LoadingScreenControl(Message,CallBbackFunctionAfterExitingLoad)
    {
        var LoadingScreenPanel = document.getElementById("LoadingScreenPanel");
        $(LoadingScreenPanel).empty();
        var CalBodyContainer = document.getElementById("CalBodyContainer");;
        this.Dom = LoadingScreenPanel;
        var myCounter = LoadingScreenControl.Counter++;
        var LoadScreenImageID = "LoadingScreenImage" + myCounter;
        var LoadScreenMessageContainerID = "LoadScreenMessageContainer" + myCounter;
        this.LoadScreenMessageContainer = getDomOrCreateNew(LoadScreenMessageContainerID);

        var LoadScreenMessageID = "LoadScreenMessage" + myCounter;
        var LoadScreenMessage = getDomOrCreateNew(LoadScreenMessageID, "span");
        LoadScreenMessage.Dom.innerHTML = Message;
        $(LoadScreenMessage.Dom).addClass("LoadScreenMessage")
        var LaodingImageDom = getDomOrCreateNew(LoadScreenImageID);
        this.LaodingImage = LaodingImageDom;
    
        $(LaodingImageDom.Dom).addClass(CurrentTheme.LoadingImage);
        $(LaodingImageDom.Dom).addClass("LoadingImage");


        var ImageAndTextContainerID = "LoadScreenImageAndTextContainer" + myCounter;
        var ImageAndTextContainer = getDomOrCreateNew(ImageAndTextContainerID);
        $(ImageAndTextContainer.Dom).addClass("LoadScreenImageAndTextContainer");

        ImageAndTextContainer.Dom.appendChild(LoadScreenMessage.Dom);
        ImageAndTextContainer.Dom.appendChild(LaodingImageDom.Dom);
    
        if(!!LoadingScreenPanel) {
            LoadingScreenPanel.appendChild(ImageAndTextContainer.Dom);
        }
        else {
            return;
        }
        

        LaunchImage();
    
        this.Launch = function (TimeBeforeLaunch, StayOnScreen)
        {
            if (TimeBeforeLaunch == null)
            {
                TimeBeforeLaunch = 0;
            }

            setTimeout(displayScreen, TimeBeforeLaunch);
        
            function displayScreen()
            {
                $(LoadingScreenPanel).show();
                LoadingScreenPanel.style.display = "block";
                //$(CalBodyContainer).addClass("blurAll");

            
            }
        }

        function LaunchImage()
        {
            var cl = new CanvasLoader(LoadScreenImageID);
            cl.setColor('#707070'); // default is '#000000'
            cl.setDiameter(200); // default is 40
            cl.setDensity(90); // default is 40
            cl.setRange(1.1); // default is 1.3
            cl.setFPS(33); // default is 24
            cl.show(); // Hidden by default
            // This bit is only for positioning - not necessary
            var loaderObj = document.getElementById("canvasLoader");
            loaderObj.style.position = "absolute";
            loaderObj.style["top"] = cl.getDiameter() * -0.5 + "px";
            loaderObj.style["left"] = cl.getDiameter() * -0.5 + "px";
        }

        function Hide(TimeBeforeLaunch, CallBbackFunctionAfterExitingLoad)
        {
            if (TimeBeforeLaunch == null)
            {
                TimeBeforeLaunch = 1200;
            }

            setTimeout(function () { innerHide(CallBbackFunctionAfterExitingLoad) }, TimeBeforeLaunch);

            function innerHide(CallBbackFunctionAfterExitingLoad)
            {
                //LoadingScreenPanel.style.display = "none";
                $(LoadingScreenPanel).hide()
                //$(CalBodyContainer).removeClass("blurAll");
                $(LoadingScreenPanel).empty();
                if (CallBbackFunctionAfterExitingLoad != undefined)
                {
                    CallBbackFunctionAfterExitingLoad();
                }
            
            }
        }

        function UpdateMessage(NewMessage, ExitAfter, CallBbackFunctionAfterExitingLoad)
        {
            LoadScreenMessage.Dom.innerHTML = NewMessage;
            if (ExitAfter.ExitNow)
            {
                Hide(ExitAfter.Delay, CallBbackFunctionAfterExitingLoad);
            }
        }

        this.UpdateMessage = UpdateMessage;

        this.Hide = Hide;
    
    }
    LoadingScreenControl.Counter = 0;

/*
*Creates an auto suggest Control Object.
*Full URI to get to desired resource
*Method Restful api request, POST,PUT, GET, DELETE, Defaults to POST
*GenerateEachDomCallBack: CallBack after response has been received. Callback is also passed the response data
*UserInputBox: Input Box DOM Element. If Null/undefined, Autosuggestcontrol creates one. This has to be an input box.
*IsNotTilerEndPoint: boolean flag to check if data expected is formated from a tile endpoint
*DataStructure: Extra data element needed sending autosuggestion request

*/
    function AutoSuggestControl(Url,Method, GenerateEachDomCallBack, UserInputBox,IsNotTilerEndPoint,DataStructure)
    {
        var myID = AutoSuggestControl.Counter++;
        var InputBarContainerID = "InputBarContainer" + myID++;
        var InputBarContainer = getDomOrCreateNew(InputBarContainerID);
        var myRequest = null;
        var DomAndContainer = generateFullInputBar(UserInputBox, IsNotTilerEndPoint,DataStructure);
        var IsContentOn = false;
        var SendRequest = true;

        var InputBarAndContentContainerID = "InputBarAndContentContainer" + myID;
        var InputBarAndContentContainer = getDomOrCreateNew(InputBarAndContentContainerID);
        $(InputBarAndContentContainer.Dom).addClass("InputBarAndContentContainer")
        if (UserInputBox==undefined)
        {
            InputBarAndContentContainer.Dom.appendChild(DomAndContainer.InputBar.Dom);
        }
        InputBarAndContentContainer.Dom.appendChild(DomAndContainer.returnedValue.Dom);
        InputBarContainer.Dom.appendChild(InputBarAndContentContainer.Dom);
    

        this.getInputBox = function ()
        {
            return DomAndContainer.InputBar.Dom;
        }

        this.getSuggestedValueContainer = function () {
            return DomAndContainer.returnedValue.Dom;
        }

        this.getAutoSuggestControlContainer =function()
        {
            return InputBarContainer.Dom;
        }


        function cancelRequest()
        {
            if (myRequest != null)
            {
                myRequest.abort();
                myRequest = null;
            }
        }

        this.cancelRequest = cancelRequest;

        this.getAutoSuggestControlID = function ()
        {
            return myID;
        }


        /*
        Function emmpties the Dom element containing the list of returned values. It also sets the content status to false;
        */
        var clear = function () {
            //debugger;
            if (InputBarContainer.Dom.parentNode != null) {
                $(InputBarContainer.Dom).empty();
                //InputBarContainer.Dom.parentNode.removeChild(InputBarContainer.Dom);
            }
            IsContentOn = false;
        }
        this.clear = clear


        function HideContainer()
        {
            //debugger;
            $(InputBarContainer.Dom).addClass("setAsDisplayNone");
            IsContentOn = false;
        }

        function ShowContainer()
        {
            $(InputBarContainer.Dom).removeClass("setAsDisplayNone");
            IsContentOn = true;
        }

        function HideAutoSuggestResult() {
            $(DomAndContainer.returnedValue).addClass("setAsDisplayNone");
        }

        
        function ShowAutoSuggestResult() {
            $(DomAndContainer.returnedValue).removeClass("setAsDisplayNone");
        }

        this.HideContainer = HideContainer;
        this.ShowContainer = ShowContainer;

        this.HideAutoSuggestResult = HideAutoSuggestResult;
        this.ShowAutoSuggestResult = ShowAutoSuggestResult;

        var IsTilerAutoContentOn = function ()
        {
            return IsContentOn;
        }
        this.isContentOn = IsTilerAutoContentOn;

        function generateFullInputBar(UserInputBox, IsNotTilerEndPoint, DataStructure)
        {
            var InputBarID = "InputBar" + AutoSuggestControl.Counter;
            var InputBar = getDomOrCreateNew(InputBarID, "input");
            if (UserInputBox != undefined)
            {
                InputBar.Dom = UserInputBox
            }
            $(InputBar.Dom).addClass("InputBar");
            var returnedValueContainerID = "returnedValueContainer" + AutoSuggestControl.Counter;
            var returnedValueContainer = getDomOrCreateNew(returnedValueContainerID);
            $(returnedValueContainer.Dom).addClass("returnedValueContainer");
            //$(returnedValueContainer.Dom).addClass(CurrentTheme.ContentSection);
            //(InputBar.Dom).oninput = prepCall(InputBar.Dom, Url, Method, returnedValueContainer, IsNotTilerEndPoint);
            $(InputBar.Dom).on("input", prepCall(InputBar.Dom, Url, Method, returnedValueContainer, IsNotTilerEndPoint, DataStructure));
            //InputBar.onchange= clear;

            var retValue = { InputBar: InputBar, returnedValue: returnedValueContainer };
            return retValue;
        }

        function disableSendRequest()
        {
            SendRequest = false;
        }

        function enableSendRequest() {
            SendRequest = true;
        }

        this.enableSendRequest = enableSendRequest;
        this.disableSendRequest = disableSendRequest;
        function getSendRequestStatus()
        {
            return SendRequest;
        }

        var ini_TimerResetID = -67767;
        var TimerResetID = ini_TimerResetID;
        var CurrentRequests = {};

        $("document").mouseup(function (e) {
            var container = $(InputBarContainer.Dom);
            if (!container.is(e.target) // if the target of the click isn't the container...
                && container.has(e.target).length === 0) // ... nor a descendant of the container
            {
                DomAndContainer.returnedValue.Dom.style.height = 0;
                $(DomAndContainer.returnedValue.Dom).empty();
            }
        })



        function prepCall(InputDom, url, Method, SuggestedValuesContainer, IsNotTilerEndPoint, DataStructure)//[reps timer
        {
            return function(e)
            {
                if (e.which == 27)
                {
                    clear();
                    return;
                }
                if (!getSendRequestStatus())
                {
                    return;
                }
                if (TimerResetID == ini_TimerResetID)
                {
                    ;
                }
                else
                {
                    clearTimeout(TimerResetID);
                }
                TimerResetID = setTimeout(prepCalToBackEnd(InputDom, url, Method, SuggestedValuesContainer, e, IsNotTilerEndPoint, DataStructure), 50);
            }
        }
        function prepCalToBackEnd(InputDom, url, Method, SuggestedValuesContainer, e, IsNotTilerEndPoint,DataStructure) {
            return function ()
            {
                //debugger;
                
                cancelRequest();//cancels any preceeding requests
                var FullLetter = InputDom.value;
                FullLetter = FullLetter.trim();
                myRequest = null;
                if (!(url))//checks if data set is already provided. If it is provided then it should just call the call back.
                {
                    //debugger;
                    var Data = url;
                    var AllDom = GenerateEachDomCallBack(Data, SuggestedValuesContainer, InputDom);
                    TimerResetID = ini_TimerResetID;
                    return;
                }

                if (FullLetter == "")
                {
                    var AllDom = GenerateEachDomCallBack([], SuggestedValuesContainer, InputDom);
                    return;
                }
                var postData ={}
                if (IsNotTilerEndPoint)
                {
                    debugger;
                    postData = DataStructure;
                    postData["query"] = FullLetter;
                }
                else
                {
                    postData={ Data: FullLetter, UserName: UserCredentials.UserName, UserID: UserCredentials.ID };
                }
                if (Method == null)
                {
                    Method="POST"
                }
            
                myRequest = $.ajax({
                    type: Method,
                    url: url,
                    //jsonp: "callback",
                    //dataType: 'jsonp',
                    data: postData/*,
                success: function (response)
                {
                    debugger;
                    response = JSON.parse(response);
                    var data = response.Content;
                    var AllDom = GenerateEachDomCallBack(data, SuggestedValuesContainer);
                }*/
                }).done(function (response)
                {
                    //response = JSON.parse(response);
                    
                    var data = IsNotTilerEndPoint ? response : response.Content;
                    var AllDom = GenerateEachDomCallBack(data, SuggestedValuesContainer, InputDom);
                    TimerResetID = ini_TimerResetID;
                    myRequest = null;
                    IsContentOn = true;
                });;

                //var returnedValue = getReturnedValueContainer(FullLetter);
                //var retValue = { InputBar: InputBar, returnedValue: returnedValue }
            }
        }
    }
    function encaseDivDomInRow(myDivDom)//function takes a div and inserts it in a row. Just incase you need the formatting of a table
    {
        var ID = encaseDivDomInRow.ID++;
        var tableRow = document.createElement("EncaseRow" + ID);
        var tableColumn = document.createElement("EncaseColumn" + ID);
        tableColumn.appendChild(myDivDom);
        tableRow.appendChild(tableColumn);
        return tableRow;
    }






    encaseDivDomInRow.ID = 0;
    AutoSuggestControl.Counter = 0;

    function getMobileOperatingSystem() {
        var userAgent = navigator.userAgent || navigator.vendor || window.opera;

        if( userAgent.match( /iPad/i ) || userAgent.match( /iPhone/i ) || userAgent.match( /iPod/i ) )
        {
            return 'iOS';

        }
        else if( userAgent.match( /Android/i ) )
        {

            return 'Android';
        }
        else
        {
            return 'unknown';
        }
    }


    function getDirectionsCallBack(EventID, CurrentTheme) {//remove hack alert
        return function () {

            LaunchMapInformation(CurrentTheme, EventID);

            /*
            var TimeZone = new Date().getTimezoneOffset();
            var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: EventID ,TimeZoneOffset: TimeZone };
    
            $.ajax({
                type: "POST",
                url: "RootWagTap/time.top?WagCommand=9",
                data: NowData,
                // DO NOT SET CONTENT TYPE to json
                // contentType: "application/json; charset=utf-8", 
                // DataType needs to stay, otherwise the response object
                // will be treated as a single string
                dataType: "json",
                success: function (response) {
                    //InitializeHomePage();
                }
            }).done(function (data) {
            });*/
        }
    }


    function OutClickManager()
    {
        var AllElements = new Array();
        document.addEventListener("mouseup", outsideClick);

        this.AddNewElement = function (NewElement)
        {
            AllElements.push(NewElement);
        }


        function outsideClick(e)
        {

            AllElements.forEach(
                function (currentElement)
                {
                    var container = $(currentElement);
                    if (!container.is(e.target) // if the target of the click isn't the container...
                        && container.has(e.target).length === 0) // ... nor a descendant of the container
                    {
                        removeElement(currentElement);
                        AllElements.pop();
                    }

                })
        }

        function removeElement(e)
        {
            e.parentNode.removeChild(e);
        }
    }

    function getFullTimeFromEntrytoJSDateobj(Obj) {
        var Time = Obj.Time;

        var DateData = new Date(Obj.Date);
        DateData.setHours(Time.Hour, Time.Minute);
        return DateData;
    }


    function generateMyButton(CallBackFunction, ButtonID) {
        //Enable Custom
        if (ButtonID == null) {
            ButtonID = "ButtonContainer" + generateMyButton.ID;
        }

        ++generateMyButton.ID;
        var EnableCustomButtonContainerID = ButtonID;
        var EnableCustomButtonContainer = getDomOrCreateNew(EnableCustomButtonContainerID);
        $(EnableCustomButtonContainer.Dom).addClass("EnableButtonContainer");
        EnableCustomButtonContainer.status = 0;


        var EnableCustomButtonID = "EnableCustomButton" + generateMyButton.ID;
        var EnableCustomButton = getDomOrCreateNew(EnableCustomButtonID);
        $(EnableCustomButton.Dom).addClass("EnableButton");

        EnableCustomButton.status = EnableCustomButtonContainer.status;

        //Enabled Recurring Settings
        var EnabledCustomContainerID = "EnabledCustomContainer" + generateMyButton.ID;
        var EnabledCustomContainer = getDomOrCreateNew(EnabledCustomContainerID);


        var EnableCustomYesTextID = "EnableCustomYesText" + generateMyButton.ID;
        var EnableCustomYesText = getDomOrCreateNew(EnableCustomYesTextID);
        EnableCustomButtonContainer.Dom.appendChild(EnableCustomYesText.Dom);
        $(EnableCustomYesText.Dom).addClass("EnableButtonChoiceText");
        $(EnableCustomYesText.Dom).addClass("EnableButtonChoiceYeaText");


        var EnableCustomNoTextID = "EnableCustomNoText" + generateMyButton.ID;
        var EnableCustomNoText = getDomOrCreateNew(EnableCustomNoTextID);
        EnableCustomButtonContainer.Dom.appendChild(EnableCustomNoText.Dom);
        $(EnableCustomNoText.Dom).addClass("EnableButtonChoiceText");
        $(EnableCustomNoText.Dom).addClass("EnableButtonChoiceNayText");
        EnableCustomButtonContainer.Dom.appendChild(EnableCustomButton.Dom);
        $(EnableCustomButtonContainer.Dom).click(ButtonClick);
        $(EnableCustomButtonContainer.Dom).attr('tabindex', 0).focus(onSlliderFocus);


        function ButtonClick() {

            EnableCustomButtonContainer.status += 1;
            EnableCustomButtonContainer.status %= 2;
            switch (EnableCustomButtonContainer.status) {
                case 0:
                    {
                        EnableCustomButtonContainer.SetAsOff();
                    }
                    break;
                case 1:
                    {
                        EnableCustomButtonContainer.SetAsOn()
                    }
                    break;
            }
        }
        function onSlliderFocus()
        {
            //debugger;
            EnableCustomButtonContainer.Dom.onkeypress=(onKeyPress);
            function onKeyPress(e)
            {
                if (e.which == 32)
                {
                    ButtonClick();
                    EnableCustomButtonContainer.Dom.focus();
                }
                if (e.which == 9)
                {
                    debugger;
                    document.removeEventListener("keypress", onKeyPress);
                }
            }
        }

    

        EnableCustomButtonContainer.toggle = ButtonClick;

        EnableCustomButtonContainer.init = function (status) {
            EnableCustomButtonContainer.status = status;
            EnableCustomButtonContainer.status += 1;
            EnableCustomButtonContainer.status %= 2;
            EnableCustomButtonContainer.toggle();
        }

        EnableCustomButtonContainer.SetAsOn = function ()
        {
            EnableCustomButtonContainer.status = 1;
            $(EnableCustomButton.Dom).removeClass("EnableButtonOff");
            $(EnableCustomButton.Dom).addClass("EnableButtonOn");
            if (CallBackFunction)
            { CallBackFunction(); }
        }

        EnableCustomButtonContainer.SetAsOff = function () {
            EnableCustomButtonContainer.status = 0;
            $(EnableCustomButton.Dom).removeClass("EnableButtonOn");
            $(EnableCustomButton.Dom).addClass("EnableButtonOff");
            if (CallBackFunction)
            { CallBackFunction(); }
        }

        return EnableCustomButtonContainer;
    }



    function generateColorPickerContainer(loopBackFunction,isHorizontalPicker) {
        var ColorPickerContainer = getDomOrCreateNew("ColorPickerContainer");
        
        //isHorizontalPicker = true;
        var ColorPicker = "";
        if (isHorizontalPicker) {
            ColorPicker = "HorizontalColorPicker";
            $(ColorPickerContainer).addClass("HorizontalColorPickerContainer")
        }
        else
        {
            ColorPicker = "ColorPicker";
        }
         //loopBackFunction

        var AllColors = new Array();
        for (var i = 0; i < global_AllColorClasses.length; i++) {
            var myClass = global_AllColorClasses[i];
            myClass.Selection = i;
            var Left = i % 3;
            var Top = Math.floor(i / 3);
            var ColorContainer = generateColorCircle();
            ColorContainer.data = myClass;
            $(ColorContainer.Selector.myColor).addClass(myClass.cssClass);
            $(ColorContainer.Selector.Container).addClass(ColorPicker);
            ColorContainer.Selector.Container.setAttribute('tabindex', 0)
            ColorContainer.Selector.Container.onkeypress = keyEntry
            AllColors.push(ColorContainer);
            //ColorContainer.Selector.Container.style.left = (Left * 33) + "%";
            //ColorContainer.Selector.Container.style.top = (Top * 33) + "%";
            ColorPickerContainer.Dom.appendChild(ColorContainer.Selector.Container);
        }

        for (var i = 0; i < AllColors.length; i++) {
            var MyCOntainer = AllColors[i];
            (MyCOntainer.Selector.Container).onclick=(genMoveOuterOrb(i))
        }

        $(AllColors[0].Selector.Container).trigger("click");

        function keyEntry(e)
        {
            if (e.which == 32)
            {
                $(e.target).trigger("click");
                return;
            }
        }

        function genMoveOuterOrb(j) {
            return function () {
                for (var i = 0; i < AllColors.length; i++) {
                    var MyCOntainer = AllColors[i];
                    MyCOntainer.Selected = false;
                    //$(MyCOntainer.Selector.OuterOrb).addClass("removeCircleAround");
                    $(MyCOntainer.Selector.OuterOrb).removeClass("addCircleAround");
                }
                AllColors[j].Selected = true;
                $(AllColors[j].Selector.OuterOrb).addClass("addCircleAround");

                if (loopBackFunction!=null)
                {
                    var ColorData = { ColorIndex: j, ColorClass: global_AllColorClasses[j].cssClass };
                    loopBackFunction(ColorData);
                }

            }
        }


        function getSelectedColor() {
            for (var i = 0; i < AllColors.length; i++) {
                var MyCOntainer = AllColors[i];
                //MyCOntainer.Selected=false;
                if (MyCOntainer.Selected === true) {
                    return MyCOntainer.data;
                }
            }
        }
        ColorPickerContainer.Selector = { Container: ColorPickerContainer.Dom, AllColors: AllColors, getColor: getSelectedColor };
        return ColorPickerContainer;
    }

    function generateColorCircle() {
        var ID = generateColorCircle.ID++;
        var ColorPickerContainer = getDomOrCreateNew("ColorContainer" + ID);
        var OuterBlackColor = getDomOrCreateNew("OuterBlackColor" + ID);
        var innerColor = getDomOrCreateNew("innerColor" + ID);
        $(innerColor.Dom).addClass("innerColor");
        $(OuterBlackColor.Dom).addClass("OuterBlackColor");



        ColorPickerContainer.Dom.appendChild(OuterBlackColor.Dom);
        ColorPickerContainer.Dom.appendChild(innerColor.Dom);

        ColorPickerContainer.Selector = { Container: ColorPickerContainer.Dom, OuterOrb: OuterBlackColor.Dom, myColor: innerColor.Dom }
        return ColorPickerContainer;
    }
    
    function generateCompletionMap(SelectedEvent)
{
    var CompletionMapID = "CompletionMap";
    var CompletionMap = getDomOrCreateNew(CompletionMapID);
    generatePieChart(CompletionMap, SelectedEvent);

    




    function generatePieChart(getDomObj, myEvent)
    {
        $(getDomObj).empty();
        var pieChartContainerID = "pieChartContainer"+generateUUID();
        var pieChartContainer = getDomOrCreateNew(pieChartContainerID,"canvas");
        var LegendContainerID = "LegendContainer";
        var LegendContainer = getDomOrCreateNew(LegendContainerID);
        $(LegendContainer.Dom).addClass("LegendContainer");
        $(pieChartContainer.Dom).addClass("pieChartContainer");

        


        var ctx = pieChartContainer.Dom;
        ctx = ctx.getContext("2d");

        var TotalNumberOfTask = parseInt(myEvent.Split)
        var DelededEvents = parseInt(myEvent.DeletionCount);
        var NumberOfCompleteTask = parseInt(myEvent.CompletionCount);

        var CompletedTask={
            value: NumberOfCompleteTask,
            color: "#28cc6a",
            hoverColor: "#2FEF7C",
            label: 'Completed Tasks'
        };
        var CompletedTaskLegend = makeMyLegend("CompletedTask");
        CompletedTaskLegend[1].Dom.innerHTML = "Completed";
        LegendContainer.Dom.appendChild(CompletedTaskLegend[2].Dom);
        CompletedTaskLegend[2].Dom.style.top = "0";
        CompletedTaskLegend[0].Dom.style.backgroundColor = CompletedTask.color;

        var DisabledTask={
            value: DelededEvents,
            color: "#BA003E",
            hoverColor: "#d60047",
            label: 'Deleted Tasks'
        };
        var DisabledTaskLegend = makeMyLegend("DisabledTask");
        DisabledTaskLegend[1].Dom.innerHTML = "Disabled";
        LegendContainer.Dom.appendChild(DisabledTaskLegend[2].Dom);
        DisabledTaskLegend[2].Dom.style.top = "33%";
        DisabledTaskLegend[0].Dom.style.backgroundColor = DisabledTask.color;

        var NotCompleted =
            {
                value: TotalNumberOfTask - (DelededEvents + NumberOfCompleteTask),
                color: "#6B6D70",
                hoverColor: "#b5b8bc",
                label: 'Available Tasks'
            };
        var NotCompletedLegend = makeMyLegend("NotCompleted");
        NotCompletedLegend[1].Dom.innerHTML = "Not Completed";
        LegendContainer.Dom.appendChild(NotCompletedLegend[2].Dom);
        NotCompletedLegend[2].Dom.style.top = "66%";
        NotCompletedLegend[0].Dom.style.backgroundColor = NotCompleted.color;


        let data = {
            labels: [
                CompletedTask.label,
                DisabledTask.label,
                NotCompleted.label
            ],
            datasets: [
                {
                    data: [CompletedTask.value, DisabledTask.value, NotCompleted.value],
                    backgroundColor: [
                        CompletedTask.color,
                        DisabledTask.color,
                        NotCompleted.color
                    ],
                    hoverBackgroundColor: [
                        CompletedTask.hoverColor,
                        DisabledTask.hoverColor,
                        NotCompleted.hoverColor
                    ]
                }]
        };

        var myDoughnutChart = new Chart(ctx, {
            type: 'doughnut',
            data: data,
            options: {
                responsive: false,
                legend: { 
                    display: false,
                    position: 'right'
                }
            }
        });


        getDomObj.Dom.appendChild(pieChartContainer.Dom);
        getDomObj.Dom.appendChild(LegendContainer.Dom);

        $(pieChartContainer.Dom).addClass("PieChart");
    }

    function makeMyLegend(ID)
    {
        var Color = getDomOrCreateNew(ID + "Color");
        $(Color.Dom).addClass("LegendColor");
        var Text = getDomOrCreateNew(ID + "Text");
        $(Text.Dom).addClass("LegendText");
        var EncasingDom = getDomOrCreateNew(ID + "LegendEncasing");
        $(EncasingDom.Dom).addClass("LegendEncasingDom");
        EncasingDom.Dom.appendChild(Color.Dom);
        EncasingDom.Dom.appendChild(Text.Dom);

        return [Color, Text, EncasingDom];
    }
    return CompletionMap.Dom;

}

    function BindDatePicker(InputDom, format) {
        if (format == null) {
            format = 'm/d/yyyy';
        }
        ///*
        $(InputDom).datepicker({
            'format': format,
            'autoclose': true
        });
        //*/

        return $(InputDom).datepicker();
    }

    function BindTimePicker(InputDom) {
        $(InputDom).timepicker({
            'showDuration': true,
            'timeFormat': 'g:ia'
        });
        return $(InputDom).timepicker();
    }


    /*
    *Function binds the "somethinew" button to the back end for a simple rest call
    */
    function SomethingNewButton(shuffleButton,callback) {
        var FindSomethingNewButton = shuffleButton;

        function reoptimizeSchedule() {
            var TimeZone = new Date().getTimezoneOffset();
            var ShuffleData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, TimeZoneOffset: TimeZone, Longitude: global_PositionCoordinate.Longitude, Latitude: global_PositionCoordinate.Latitude, IsInitialized: global_PositionCoordinate.isInitialized };
            var URL = global_refTIlerUrl + "Schedule/Shuffle";
            var HandleNEwPage = new LoadingScreenControl("Tiler looking up the next good event  :)");
            HandleNEwPage.Launch();
            ShuffleData.TimeZone = moment.tz.guess()
            preSendRequestWithLocation(ShuffleData);
            var exit = function (data) {
                HandleNEwPage.Hide();
                global_ExitManager.triggerLastExitAndPop();
            }
            $.ajax({
                type: "POST",
                url: URL,
                data: ShuffleData,
                // DO NOT SET CONTENT TYPE to json
                // contentType: "application/json; charset=utf-8", 
                // DataType needs to stay, otherwise the response object
                // will be treated as a single string
                dataType: "json",
                success: function (response) {
                    triggerUndoPanel("Undo optimized shuffle");
                    var myContainer = (response);
                    if (myContainer.Error.code == 0) {
                        if (isFunction(callback)) {
                            callback(response);
                        }
                    }
                    else {
                        alert("error optimizing your schedule");
                    }

                },
                error: function () {
                    var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                    var ExitAfter = {
                        ExitNow: true, Delay: 1000
                    };
                    HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exit);
                }
            }).done(function (data) {
                HandleNEwPage.Hide();
                sendPostScheduleEditAnalysisUpdate({});
            });
        }
        FindSomethingNewButton.onclick = reoptimizeSchedule;
    }


    function removeAllChildNodes(myNode) {
        while (myNode.firstChild) {
            myNode.removeChild(myNode.firstChild);
        }
    }


    function destroyallNodes(Node) {
        while (Node.firstChild) {
            recursiveDeletion(Node.firstChild)
        }
        function recursiveDeletion(Node) {
            while (Node.firstChild) {
                destroyallNodes(Node)
            }
            if (!!Node.parentNode) {
                removeAllChildNodes(Node.parentNode);
            }
        }
    }

    function isFunction(data) {
        var RetValue = false;
        if (typeof (data) === "function") {
            RetValue = true;
        }
        return RetValue;
    }

    function isObject(data) {
        var RetValue = false;
        if ((typeof (data) === "object") && (data !== null)) {
            RetValue = true;
        }
        return RetValue;
    }

    function isString(data) {
        var RetValue = false;
        if ((typeof (data) === "string") && (data !== null)) {
            RetValue = true;
        }
        return RetValue;
    }

    function isNumber(data) {
        var RetValue = false;
        if ((typeof (data) === "number") && (data !== null)) {
            RetValue = true;
        }
        return RetValue;
    }


    function isNull(data) {
        var RetValue = false;
        if ((typeof (data) === "object") && (data === null)) {
            RetValue = true;
        }
        return RetValue;
    }

    function isUndefined(data) {
        var RetValue = false;
        if ((typeof (data) === "undefined") && (data !== null)) {
            RetValue = true;
        }
        return RetValue;
    }

    function isUndefinedOrNull(data) {
        var RetValue = false;
        if (isUndefined(data) || isNull(data)) {
            RetValue = true;
        }

        return RetValue;
    }

    generateColorCircle.ID = 0;



    generateMyButton.ID = 0;


function isFunction(data) {
    var RetValue = false;
    if (typeof (data) === "function") {
        RetValue = true;
    }
    return RetValue;
}

function isObject(data) {
    var RetValue = false;
    if ((typeof (data) === "object") && (data !== null)) {
        RetValue = true;
    }
    return RetValue;
}

function isString(data) {
    var RetValue = false;
    if ((typeof (data) === "string") && (data !== null)) {
        RetValue = true;
    }
    return RetValue;
}

function isNumber(data) {
    var RetValue = false;
    if ((typeof (data) === "number") && (data !== null)) {
        RetValue = true;
    }
    return RetValue;
}


function isNull(data) {
    var RetValue = false;
    if ((typeof (data) === "object") && (data === null)) {
        RetValue = true;
    }
    return RetValue;
}

function isArray(data) {
    var RetValue = Array.isArray(data);
    return RetValue;
}

function isUndefined(data) {
    var RetValue = false;
    if ((typeof (data) === "undefined") && (data !== null)) {
        RetValue = true;
    }
    return RetValue;
}

function isUndefinedOrNull(data) {
    var RetValue = false;
    if (isUndefined(data) || isNull(data)) {
        RetValue = true;
    }

    return RetValue;
}


    /*
    Function handles the call back for the autoSuggest Box of location
    */
   function LocationSearchCallBack(ExitCallBack, InputBox, onSelctionMade)
   {
       $(InputBox).off();
       var AutoSuggestEndPoint = global_refTIlerUrl + "User/Location";
       //var GoogleAutoSuggestEndPoint = "https://maps.googleapis.com/maps/api/place/textsearch/json";
       var GoogleAutoSuggestEndPoint = "";

       var googleSendData = {};
       
       googleSendData.key = googleAPiKey
       googleSendData.query = "";
       
       var LocationAutoSuggestControl = new AutoSuggestControl(AutoSuggestEndPoint, "GET", AddressCallBack, InputBox);
       var GoogleAutoSuggestControl = new AutoSuggestControl(GoogleAutoSuggestEndPoint, "GET", googleAddressCallBack, InputBox, true, googleSendData);
       var MyDataContainer = { AllData: [], Index: -1 };
       var GoogleDataContainer = { AllData: [], Index: -1 };
       var CombinedData= { AllData: [], Index: -1 };
       var FullContainer = LocationAutoSuggestControl.getAutoSuggestControlContainer();


       //Combined callback
       function combinedCallBack(typeOfData,MyData)
       {
           //if (combinedCallBack.currentIndex == 0)
           if (typeOfData == 0)
           {
               combinedCallBack.cleanUI();
               LaunchPopulation(typeOfData)
           }
           else
           {
               combinedCallBack.indexContainer[typeOfData] = MyData;
               return;
           }

           
           function LaunchPopulation(Index)
           {
               if (combinedCallBack.indexContainer[Index]!=null)
               {
                   combinedCallBack.indexContainer[Index].forEach
                   (
                       function (myData)
                       {
                           CombinedData.AllData.push(myData);
                           myData.Index = LaunchPopulation.Index;
                           ++LaunchPopulation.Index;
                           myData.Hover = HoverMe;
                           function HoverMe()
                           {
                               if (combinedCallBack.CurrentHover != null) {
                                   combinedCallBack.CurrentHover.UnHover();
                               }
                               combinedCallBack.CurrentHover = myData;
                               //MyDataContainer.Index = Index;
                               $(myData.Container).addClass("HoveLocationCacheContainer");
                           }
                       }
                   )
                   combinedCallBack.indexContainer[Index] = null;
                   ++Index;
                   combinedCallBack.currentIndex = Index;
                   LaunchPopulation(combinedCallBack.currentIndex);
               }
               else
               {
                   if (combinedCallBack.currentIndex >= combinedCallBack.indexContainer.length)
                   {
                       combinedCallBack.currentIndex = 0;
                       combinedCallBack.clearData();
                       PopulateteContainerDom();
                       return;
                   }
                   else
                   {
                       setTimeout(function () { LaunchPopulation(combinedCallBack.currentIndex) }, 200);
                   }
               }
           }

           function PopulateteContainerDom()
           {
               for (var i=0;i<CombinedData.AllData.length;i++)
               {
                   var Data=CombinedData.AllData[i];
                   justPushIntoContainer(Data);
               }
               
               function justPushIntoContainer(myData)
               {
                   combinedCallBack.DomContainer.Dom.appendChild(myData.Container);
               }
               
           }
           LaunchPopulation.Index = 0;

       }

       combinedCallBack.CurrentHover = null;
       combinedCallBack.indexContainer = [null,null];
       combinedCallBack.currentIndex = 0;

       combinedCallBack.DomContainer = LocationAutoSuggestControl.getSuggestedValueContainer();

       combinedCallBack.rePopulate = function () {

       }



       combinedCallBack.clear = function ()
       {
           MyDataContainer.AllData.splice(0, MyDataContainer.AllData.length)
           MyDataContainer.Index = -1;
           //LocationAutoSuggestControl.clear();
           LocationAutoSuggestControl.HideContainer();

           GoogleDataContainer.AllData.splice(0, GoogleDataContainer.AllData.length)
           GoogleDataContainer.Index = -1;
           //LocationAutoSuggestControl.clear();
           GoogleAutoSuggestControl.HideContainer();

           CombinedData.AllData.splice(0, CombinedData.AllData.length);
           CombinedData.Index = -1;
           $(combinedCallBack.DomContainer).empty();
           //LocationAutoSuggestControl.clear();
           LocationAutoSuggestControl.HideContainer();
       }

       combinedCallBack.clearData=function()
       {
           MyDataContainer.AllData.splice(0, MyDataContainer.AllData.length)
           MyDataContainer.Index = -1;
           GoogleDataContainer.AllData.splice(0, GoogleDataContainer.AllData.length)
           GoogleDataContainer.Index = -1;
       }

       combinedCallBack.cleanUI = function ()
       {
           //console.log("Called Clear " + combinedCallBack.currentIndex);
           CombinedData.AllData.splice(0, CombinedData.AllData.length);
           CombinedData.Index = -1;
           $(combinedCallBack.DomContainer).empty();
       }

       //Tiler Address callback
       function AddressCallBack(data, DomContainer, InputCOntainer)
       {
           
           var FullContainer = LocationAutoSuggestControl.getAutoSuggestControlContainer();
           InputBox.parentNode.appendChild(FullContainer);
           positionSearchResultContainer();
           
           

           MyDataContainer.AllData.splice(0, MyDataContainer.AllData.length);
           
           resolveEachRetrievedEvent.ID = 0;
           

           InputBox.onblur = function () { setTimeout(function () { ReseAutoSuggest() }, 300) }
           
           LocationAutoSuggestControl.ShowContainer();

           for (var i = 0; ((i < data.length)&&(i<5)); i++)
           {
               resolveEachRetrievedEvent(data[i]);
           }

           
           var CombinedDataIndex = 0;
           combinedCallBack.indexContainer[CombinedDataIndex] = MyDataContainer.AllData;
           combinedCallBack(CombinedDataIndex, MyDataContainer.AllData);

           function ReseAutoSuggest() {
               MyDataContainer.AllData.splice(0, MyDataContainer.AllData.length)
               MyDataContainer.Index = -1;
               
               combinedCallBack.clear();
           }

           function resolveEachRetrievedEvent(LocationData)//,Index) {
           {
               var CalendarEventDom = generateDomForEach(LocationData);//,Index);
               $(CalendarEventDom.Container).addClass("LocationCacheContainer");
               //DomContainer.Dom.appendChild(CalendarEventDom.Container);
               MyDataContainer.AllData.push(CalendarEventDom);
               ++resolveEachRetrievedEvent.ID;
           }
           resolveEachRetrievedEvent.ID = 0;
           

           function generateDomForEach(LocationData)//,Index)
           {
               var TagSpan = getDomOrCreateNew(("TagSpan" + resolveEachRetrievedEvent.ID), "span");
               TagSpan.innerHTML = LocationData.Tag + " &mdash; ";

               $(TagSpan).addClass("LocationTag");
               var AddressSpan = getDomOrCreateNew(("AddressSpan " + resolveEachRetrievedEvent.ID), "span");
               AddressSpan.innerHTML = LocationData.Address;
               $(AddressSpan).addClass("LocationAddress");
               var CacheAddressContainer = getDomOrCreateNew(("CacheAddressContainer" + resolveEachRetrievedEvent.ID));
               CacheAddressContainer.appendChild(TagSpan);
               CacheAddressContainer.appendChild(AddressSpan);
               CacheAddressContainer.onclick = function () { SelectMe() };

               var RetValue = { Container: CacheAddressContainer, Hover: HoverMe, UnHover: UnHoverMe, Select: SelectMe, /*Index: Index,*/ Insert: InsertIntoInput };

               function HoverMe()
               {
                   if(generateDomForEach.CurrentHover!=null)
                   {
                       generateDomForEach.CurrentHover.UnHover();
                   }
                   generateDomForEach.CurrentHover = RetValue;
                   //MyDataContainer.Index = Index;
                   $(CacheAddressContainer).addClass("HoveLocationCacheContainer");
               }

               function UnHoverMe()
               {
                   $(CacheAddressContainer).removeClass("HoveLocationCacheContainer");
               }

               function SelectMe()
               {
                   //InputCOntainer.value = LocationData.Address;
                   InsertIntoInput();
                   LocationAutoSuggestControl.HideContainer();

                   if(isFunction(onSelctionMade)) {
                    onSelctionMade(LocationData)
                   }
                   setTimeout(function () { InputBox.focus(), 200 });
               }

               function InsertIntoInput()
               {
                   updateLocationInputWithClickData(InputCOntainer, LocationData.Address, "tiler", LocationData.LocationId)
                   if (LocationData.isVerified !== undefined && LocationData.isVerified !== null) {
                       InputCOntainer.LocationIsVerified = LocationData.isVerified;
                   }
               }

               return RetValue;
           }
           generateDomForEach.CurrentHover = null;
       }


       //Google Address callback
       function googleAddressCallBack(data, DomContainer, InputCOntainer)
       {

           function initialize()
           {
               ReseAutoSuggest();
               var dataInput = InputCOntainer.value
               dataInput = dataInput.trim();
               var defaultLocation = new google.maps.LatLng(global_PositionCoordinate.Latitude, global_PositionCoordinate.Longitude);
               var request = {
                   location: defaultLocation,
                   radius: 50,
                   query: dataInput
                   //query: "vectra bank"
               };

               var service = new google.maps.places.PlacesService(DomContainer);
               service.textSearch(request, callback);
           }

           function callback(results, status) {
               
               if (status == google.maps.places.PlacesServiceStatus.OK) {
                   for (var i = 0; ((i < results.length)&&(i<5)); i++) {
                       resolveEachRetrievedEvent(results[i], onSelctionMade);
                   }
               }

               var CombinedDataIndex = 1;
               combinedCallBack.indexContainer[CombinedDataIndex] = GoogleDataContainer.AllData;
               combinedCallBack(CombinedDataIndex, GoogleDataContainer.AllData);
           }

           initialize();

           
           

           function ReseAutoSuggest() {
               GoogleDataContainer.AllData.splice(0, GoogleDataContainer.AllData.length)
               GoogleDataContainer.Index = -1;
               //LocationAutoSuggestControl.HideContainer();
           }

           function resolveEachRetrievedEvent(LocationData, onSelctionMade)//, Index)
           {
               var CalendarEventDom = generateDomForEach(LocationData, onSelctionMade)//, Index);
               $(CalendarEventDom.Container).addClass("LocationCacheContainer");
               //DomContainer.Dom.appendChild(CalendarEventDom.Container);
               GoogleDataContainer.AllData.push(CalendarEventDom);
               ++resolveEachRetrievedEvent.ID;
           }
           resolveEachRetrievedEvent.ID = 0;
           //GoogleDataContainer.AllData[0].Hover();

           function generateDomForEach(LocationData, onSelctionMade)//, Index)
           {
               var TagSpan = getDomOrCreateNew(("GoogleTagSpan" + resolveEachRetrievedEvent.ID), "span");
               TagSpan.innerHTML = LocationData.name + " &mdash; ";

               $(TagSpan).addClass("LocationTag");
               var AddressSpan = getDomOrCreateNew(("GoogleAddressSpan " + resolveEachRetrievedEvent.ID), "span");
               AddressSpan.innerHTML = LocationData.formatted_address;
               $(AddressSpan).addClass("LocationAddress");
               var CacheAddressContainer = getDomOrCreateNew(("GoogleCacheAddressContainer" + resolveEachRetrievedEvent.ID));
               var GoogleSymbolContainer = getDomOrCreateNew(("GoogleSymbolContainer" + resolveEachRetrievedEvent.ID));
               $(GoogleSymbolContainer).addClass("GoogleSearchSymbolContainer");
               var GoogleSymbol = getDomOrCreateNew(("GoogleSymbol" + resolveEachRetrievedEvent.ID));
               $(GoogleSymbol).addClass("GoogleSearchSymbol");
               $(GoogleSymbol).addClass("GoogleSearchIcon");
               GoogleSymbolContainer.appendChild(GoogleSymbol);

               CacheAddressContainer.appendChild(TagSpan);
               CacheAddressContainer.appendChild(AddressSpan);
               CacheAddressContainer.appendChild(GoogleSymbolContainer);
               CacheAddressContainer.onclick = function () { SelectMe() };

               var RetValue = { Container: CacheAddressContainer, Hover: HoverMe, UnHover: UnHoverMe, Select: SelectMe, /*Index: Index,*/ Insert: InsertIntoInput };

               function HoverMe() {
                   if (generateDomForEach.CurrentHover != null) {
                       generateDomForEach.CurrentHover.UnHover();
                   }
                   generateDomForEach.CurrentHover = RetValue;
                   //GoogleDataContainer.Index = Index;
                   $(CacheAddressContainer).addClass("HoveLocationCacheContainer");
               }

               function UnHoverMe() {
                   $(CacheAddressContainer).removeClass("HoveLocationCacheContainer");
               }

               function SelectMe() {
                   //InputCOntainer.value = LocationData.Address;
                   InsertIntoInput();
                   LocationAutoSuggestControl.HideContainer();
                   if(isFunction(onSelctionMade)) {
                        onSelctionMade(LocationData)
                   }

                   setTimeout(function () { InputBox.focus(), 200 });
               }

               function InsertIntoInput() {
                   updateLocationInputWithClickData(InputCOntainer, LocationData.formatted_address, "google")
               }

               return RetValue;
           }
           generateDomForEach.CurrentHover = null;
       }

       function positionSearchResultContainer()
       {
           var InputBox = LocationAutoSuggestControl.getInputBox();
           var Position = $(InputBox.Dom).position();
           var Left = 50;// Position.left;
           var Top = Position.top;
           var height = $(InputBox.Dom).height();
           Top += height;
           $(FullContainer).css({ left: Left + "px", top: Top + "px", position: "absolute", width: "calc(100% - 100px)" });
       }

       
       function ReturnFunction(e, ExitFunction)
       {
           if(e.which == 27)
           {
               if (LocationAutoSuggestControl.isContentOn() || GoogleAutoSuggestControl.isContentOn())
               {
                   combinedCallBack.clear();
                   return;
               }
               else
               {
                   ExitFunction();
               }
               return;
           }

           LocationAutoSuggestControl.disableSendRequest();
           GoogleAutoSuggestControl.disableSendRequest();
           var OldIndex = CombinedData.Index;
           if(e.which == 38)//UpArrow Press
           {
               var NewIndex = ((CombinedData.Index - 1) + CombinedData.AllData.length) % CombinedData.AllData.length
               CombinedData.Index = NewIndex;

               console.log("Going up -- Old index is : " + OldIndex + " New Index is -- " + NewIndex + " Total Possible :" + CombinedData.AllData.length);
               CombinedData.AllData[CombinedData.Index].Hover();
               //CombinedData.AllData[CombinedData.Index].Insert();
               positionSearchResultContainer();
               return;
           }
           if (e.which == 40)//Down Arrow Press
           {
               var NewIndex = ((CombinedData.Index + 1) + CombinedData.AllData.length) % CombinedData.AllData.length
               CombinedData.Index = NewIndex;

               console.log("Going Down -- Old index is : " + OldIndex + " New Index is -- " + NewIndex + " Total Possible :" + CombinedData.AllData.length);
               CombinedData.AllData[CombinedData.Index].Hover();
               //CombinedData.AllData[CombinedData.Index].Insert();
               positionSearchResultContainer();
               return;
           }
           if (e.which == 13)
           {
               CombinedData.AllData[CombinedData.Index].Select();
               return;
           }
           e.target.LocationIsVerified = false
           resetLocationInput(e.target)
           LocationAutoSuggestControl.enableSendRequest();
           GoogleAutoSuggestControl.enableSendRequest();
       }

       return ReturnFunction;
   }


   function genFunctionCallForNow(EventID,CallBack)
   {
       return function ()
       {
           var TimeZone = new Date().getTimezoneOffset();
           var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: EventID, TimeZoneOffset: TimeZone };
           //var URL = "RootWagTap/time.top?WagCommand=8"
           var URL = global_refTIlerUrl + "Schedule/Event/Now";
           NowData.TimeZone = moment.tz.guess()
           var HandleNEwPage = new LoadingScreenControl("Tiler is moving up your Event ...  :)");
           HandleNEwPage.Launch();
           preSendRequestWithLocation(NowData);

           $.ajax({
               type: "POST",
               url: URL,
               data: NowData,
               // DO NOT SET CONTENT TYPE to json
               // contentType: "application/json; charset=utf-8", 
               // DataType needs to stay, otherwise the response object
               // will be treated as a single string
               dataType: "json",
               success: function (response) {
                   //InitializeHomePage();
                   //alert("alert 0-a");
               },
               error: function ()
               {
                   var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:(";
                   var ExitAfter = { ExitNow: true, Delay: 1000 };
                   HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, InitializeHomePage);
               }
           }).done(function (data) {
               if(isFunction(CallBack)) {
                    CallBack()
               }
               HandleNEwPage.Hide();
               sendPostScheduleEditAnalysisUpdate({});
           });
       }
   }