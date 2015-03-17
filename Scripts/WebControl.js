"use strict"


var Debug = false;
var DebugLocal = false;

//var global_refTIlerUrl = "http://localhost:53201/api/";
//var global_refTIlerUrl = "http://tilersmart.azurewebsites.net/api/";
var global_refTIlerUrl = window.location.origin + "/api/";
if (Debug)
{
    global_refTIlerUrl = "http://mytilerKid.azurewebsites.net/api/";
    if(DebugLocal)
    {
        global_refTIlerUrl = "https://localhost:44302/api/";
    }
}


var UserTheme = { Light: new Theme("Light"), Dark: new Theme("Dark") };
var CurrentTheme = UserTheme.Light;
var UserCredentials = {UserName:"",ID:"", Name:""};
var OneDayInMs = 86400000;
var OneWeekInMs = OneDayInMs * 7;
var FourWeeksInMs = OneWeekInMs * 4;
var OneHourInMs = 3600000;
var OneYearInMs = 365 * OneDayInMs;
var OneMinInMs = 60000;
var OneSecondInMs = 1000;


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





var global_TimeZone_ms = new Date().getTimezoneOffset()*60000;



//alert("Hi Jerome\n" + TestStart + "\n" + document.getElementById("hahahah"));



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
    var CookieValue = "";
    var DocumentCookie = " " + document.cookie + ";";
    var CookieSearchStr = " " + CookieName + "=";
    var CookieStartPosition = DocumentCookie.indexOf(CookieSearchStr);
    var CookieEndPosition;
	var CookieValue ;//= JSON.parse(CookieValue);

    if (CookieStartPosition != -1) {
        CookieStartPosition += CookieSearchStr.length;
        CookieEndPosition = DocumentCookie.indexOf(";", CookieStartPosition);
        CookieValue = decodeURI(DocumentCookie.substring(CookieStartPosition, CookieEndPosition));
        CookieValue = JSON.parse(CookieValue);
    }
    if (CookieValue == "")
    {
        if (Debug)
            {
        ///*
            CookieValue = { UserName: "jerbio", UserID: "d350ba4d-fe0b-445c-bed6-b6411c2156b3" }
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


/*
*Function generates a desired HTML element. The default it a Div. First arguement is the desited ID, returns Null if an ID is not provided. Second arguement is desired DOM type e.g span, label, button etc

*/
function getDomOrCreateNew(DomID, DomType)
{
    var retValue = { status: false, Dom: null, Misc:null };
    if ((DomID==null)||(DomID==undefined))//checks domID for valid ID
    {
        return retValue;

    }

    if (DomType == null)
    {
        DomType="div";
    }

    var myDom = document.getElementById(DomID);
    retValue.status = true;
    if (myDom == null)
    {
        myDom = document.createElement(DomType);
        myDom.setAttribute("id", DomID);
        retValue.status = false;
    }
    
    retValue.Dom = myDom;

    var JustAnotherObj = retValue;
    retValue = JustAnotherObj.Dom;
    retValue.status = JustAnotherObj.status;
    retValue.Dom = JustAnotherObj.Dom;
    retValue.Misc = JustAnotherObj.Misc;
    retValue.DomID = DomID;
    return retValue;
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
    (FullLabel.Dom).style.height = "100px";
    (FullLabel.Dom).style.position = "absolute";
    (FullLabel.Dom).style.display = "table";
    //(FullLabel.Dom).style.borderBottom = "3px solid rgb(127,127,127)";
    if (JqueryCss != null)
    {
        $(FullLabel.Dom).css(JqueryCss);
    }

    
    var LabelContainer = getDomOrCreateNew(InputParams.ID + "Label");
    var InputContainer = getDomOrCreateNew(InputParams.ID + "Input", "input");
    LabelContainer.Dom.innerHTML = InputParams.Name;
    LabelContainer.Dom.style.width = "20%";
    LabelContainer.Dom.style.left = "0%";
    LabelContainer.Dom.style.position = "absolute";

    InputContainer.Dom.style.width = "78%";
    InputContainer.Dom.style.left = "20%";
    InputContainer.Dom.style.height = "40px";
    //InputContainer.Dom.style.maxHeight = "40%";
    InputContainer.Dom.style.fontSize = "12px";
    InputContainer.Dom.style.position = "absolute";
    
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

    function transitionOldContainer(NewContainer)
    {
        //var oldContainer = GetCurrentContainer();
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
        retValue+=TimeString[i]
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


function setSubCalendarEventAsNow(SubEventID, CallBackSuccess, CallBackFailure, CallBackDone)
{
    var TimeZone = new Date().getTimezoneOffset();
    var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: SubEventID, TimeZoneOffset: TimeZone };
    var URL = myurl = global_refTIlerUrl + "Schedule/Event/Now";

    var ProcrastinateRequest= $.ajax({
        type: "POST",
        url: URL,
        data: NowData,
        dataType: "json",
        success: CallBackSuccess,
        error: CallBackFailure
    })
    
    if (CallBackDone != undefined) {
        ProcrastinateRequest.done(CallBackDone);
    }
}

function SetCalendarEventAsNow(CalendarEventID, CallBackSuccess, CallBackFailure, CallBackDone) {
    var TimeZone = new Date().getTimezoneOffset();
    var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, ID: SubEventID, TimeZoneOffset: TimeZone };
    var URL = myurl = global_refTIlerUrl + "CalendarEvent/Now/";

    var AjaxRequest = $.ajax({
        type: "POST",
        url: URL,
        data: NowData,
        dataType: "json",
        success: CallBackSuccess,
        error: CallBackFailure
    })

    if (CallBackDone != undefined) {
        AjaxRequest.done(CallBackDone);
    }
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

    if (CallBackDone != undefined)
    {
        AjaxRequest.done(CallBackDone);
    }
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

    if (CallBackDone != undefined) {
        AjaxRequest.done(CallBackDone);
    }
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

    if (CallBackDone != undefined) {
        AjaxRequest.done(CallBackDone);
    }
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

        var DayTextBox = getDomOrCreateNew("DialDayInput" + DialOnEvent.id);
        DayTextBox.Dom.innerHTML = "<p>Day(s)</p>";
        DayTextBox.Dom.setAttribute("class", "DialInput DialDayInput");
        $(DayTextBox.Dom).click(function () {
            var CurrentDayValue = new Dial(0, 1, 1, 0);
            CurrentDial = CurrentDayValue;
            $(SelectedDialOptionDomButton.Dom).removeClass("SelectedDialButton");
            SelectedDialOptionDomButton = DayTextBox;

            $(SelectedDialOptionDomButton.Dom).addClass("SelectedDialButton");
            InitializeDialDial(CurrentDayValue, EventDialContainer.Dom);
        })


        var HourTextBox = getDomOrCreateNew("DialHourInput" + DialOnEvent.id);
        HourTextBox.Dom.setAttribute("class", "DialInput DialHourInput");
        HourTextBox.Dom.innerHTML = "<p>Hour(s)</p>";
        $(HourTextBox.Dom).click(function () {
            var CurrentHourValue = new Dial(0, 5, 0, 60 * 2);
            CurrentDial = CurrentHourValue;
            $(SelectedDialOptionDomButton.Dom).removeClass("SelectedDialButton");
            SelectedDialOptionDomButton = HourTextBox;
            $(SelectedDialOptionDomButton.Dom).addClass("SelectedDialButton");
            InitializeDialDial(CurrentHourValue, EventDialContainer.Dom);
        })

        var MinTextBox = getDomOrCreateNew("DialMinInput" + DialOnEvent.id);;
        MinTextBox.Dom.setAttribute("class", "DialInput DialMinInput");
        MinTextBox.Dom.innerHTML = "<p>Min(s)</p>";



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


    function Location(Tag,Address)
    {
        this.Tag = Tag;
        this.Address = Address;
    }

    function CalEventData(eventName, eventLocation, eventCounts, eventColor, eventDuration, eventStart, eventEnd, eventRepeatData, eventRepeatStart, eventRepeatEnd, rigidFlag, RestrictionData)
    {
        this.Name = eventName;
        this.LocationTag = eventLocation.Tag;
        this.LocationAddress = eventLocation.Address;
        this.RColor = eventColor.r;
        this.GColor = eventColor.g;
        this.BColor = eventColor.b;
        this.ColorSelection = null == eventColor.s ? 0 : eventColor.s;
        this.Opacity = null == eventColor.o ? 1 : eventColor.o;
        this.DurationDays = eventDuration.Days;
        this.DurationHours = eventDuration.Hours;
        this.DurationMins = eventDuration.Mins;
        this.StartHour = eventStart.Time.Hour;
        this.EndHour = eventEnd.Time.Hour;
        this.StartMins = eventStart.Time.Minute;
        this.EndMins = eventEnd.Time.Minute;
        this.StartDay = eventStart.Date.getDate();
        this.EndDay = eventEnd.Date.getDate();
    
        this.StartMonth = eventStart.Date.getMonth()+1
        this.EndMonth = eventEnd.Date.getMonth()+1;

        this.StartYear = eventStart.Date.getFullYear();
        this.EndYear = eventEnd.Date.getFullYear();

        var isRestricted = false;
        var RestrictionStart = "12:00am"
        var RestrictionEnd = "12:00am"
        var isWorkWeek = false;

        if (RestrictionData != null)
        {
            if (RestrictionData.isRestriction)
            {
                isRestricted = true;
                RestrictionStart = RestrictionData.Start;
                RestrictionEnd = RestrictionData.End;
                isWorkWeek = RestrictionData.isWorkWeek;
            }
        }


        this.isRestricted = isRestricted;
        this.RestrictionStart = RestrictionStart;
        this.RestrictionEnd = RestrictionEnd;
        this.isWorkWeek = isWorkWeek;

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
            }
        }
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
        this.Rigid = rigidFlag?true:false;
        //alert(rigidFlag);
        this.Count = eventCounts;

    

        //{ Days: Days, Hours: Hours, Mins: Mins };

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
        var LoadingScreenPanel = document.getElementById("LoadingScreenPanel");;
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
    
        LoadingScreenPanel.appendChild(ImageAndTextContainer.Dom);

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


    function AutoSuggestControl(Url,Method, GenerateEachDomCallBack, UserInputBox)
    {
        var myID = AutoSuggestControl.Counter++;
        var InputBarContainerID = "InputBarContainer" + myID++;
        var InputBarContainer = getDomOrCreateNew(InputBarContainerID);
        var myRequest = null;
        var DomAndContainer = generateFullInputBar(UserInputBox);


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
        this.clear = function ()
        {
            if (InputBarContainer.Dom.parentNode != null)
            {
                $(InputBarContainer.Dom).empty();
                InputBarContainer.Dom.parentNode.removeChild(InputBarContainer.Dom);
            }   
        }
    
        function generateFullInputBar(UserInputBox)
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
            (InputBar.Dom).onkeyup = prepCall(InputBar.Dom, Url, Method, returnedValueContainer);
        

            var retValue = { InputBar: InputBar, returnedValue: returnedValueContainer };
            return retValue;
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



        function prepCall(InputDom, url, Method, SuggestedValuesContainer)//[reps timer
        {
            return function(e)
            {
                if (TimerResetID == ini_TimerResetID)
                {
                    ;
                }
                else
                {
                    clearTimeout(TimerResetID);
                }
                TimerResetID = setTimeout(prepCalToBackEnd(InputDom, url, Method, SuggestedValuesContainer,e), 50);
            }
        }


        function prepCalToBackEnd(InputDom, url, Method, SuggestedValuesContainer,e) {
            return function ()
            {
                //debugger;
                if (typeof (url) != "string")//checks if data set is already provided. If it is provided then it should just call the call back.
                {
                    var Data = url;
                    var AllDom = GenerateEachDomCallBack(Data, SuggestedValuesContainer, InputDom);
                    TimerResetID = ini_TimerResetID;
                    return;
                }
                cancelRequest();//cancels any preceeding requests
                var FullLetter = InputDom.value;
                FullLetter = FullLetter.trim();
                myRequest=null;
                if (FullLetter == "")
                {
                    var AllDom = GenerateEachDomCallBack([], SuggestedValuesContainer, InputDom);
                    return;
                }
                var postData = { Data: FullLetter, UserName: UserCredentials.UserName, UserID: UserCredentials.ID };
                if (Method == null)
                {
                    Method="POST"
                }
            
                myRequest = $.ajax({
                    type: Method,
                    url: url,
                    data: postData/*,
                success: function (response)
                {
                    response = JSON.parse(response);
                    var data = response.Content;
                    var AllDom = GenerateEachDomCallBack(data, SuggestedValuesContainer);
                }*/
                }).done(function (response)
                {
                    //response = JSON.parse(response);
                    var data = response.Content;
                    var AllDom = GenerateEachDomCallBack(data, SuggestedValuesContainer, InputDom);
                    TimerResetID = ini_TimerResetID;
                    myRequest = null;
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
    //$(CompletionMap.Dom).addClass("SubEventNonLabelSection");
    //$(CompletionMap.Dom).addClass(CurrentTheme.ContentSection)
    //$(CompletionMap.Dom).addClass(CurrentTheme.FontColor);

    generatePieChart(CompletionMap, SelectedEvent);

    




    function generatePieChart(getDomObj, myEvent)
    {
        var pieChartContainerID = "pieChartContainer";
        var pieChartContainer = getDomOrCreateNew(pieChartContainerID,"canvas");
        var LegendContainerID = "LegendContainer";
        var LegendContainer = getDomOrCreateNew(LegendContainerID);
        $(LegendContainer.Dom).addClass("LegendContainer");

        


        var ctx = pieChartContainer.Dom;
        ctx = ctx.getContext("2d");
        var myNewChart = new Chart(ctx);
        
        getDomObj.Dom.appendChild(pieChartContainer.Dom);
        getDomObj.Dom.appendChild(LegendContainer.Dom);

        $(pieChartContainer.Dom).addClass("PieChart");

        var TotalNumberOfTask = parseInt(Dictionary_OfCalendarData[myEvent.CalendarID].TotalNumberOfEvents)
        var DelededEvents=parseInt(Dictionary_OfCalendarData[myEvent.CalendarID].DeletedEvents);
        var NumberOfCompleteTask =parseInt(Dictionary_OfCalendarData[myEvent.CalendarID].CompletedEvents);


        var CompletedTask={
            value: NumberOfCompleteTask,
            color: "green"
        };
        var CompletedTaskLegend = makeMyLegend("CompletedTask");
        CompletedTaskLegend[1].Dom.innerHTML = "Completed";
        LegendContainer.Dom.appendChild(CompletedTaskLegend[2].Dom);
        CompletedTaskLegend[2].Dom.style.top = "0";
        CompletedTaskLegend[0].Dom.style.backgroundColor = "green";

        var DisabledTask={
            value: DelededEvents,
            color: "red"
        };
        var DisabledTaskLegend = makeMyLegend("DisabledTask");
        DisabledTaskLegend[1].Dom.innerHTML = "Disabled";
        LegendContainer.Dom.appendChild(DisabledTaskLegend[2].Dom);
        DisabledTaskLegend[2].Dom.style.top = "33%";
        DisabledTaskLegend[0].Dom.style.backgroundColor = "red";

        var NotCompleted =
            {
                value: TotalNumberOfTask - (DelededEvents + NumberOfCompleteTask),
            color: "gray"
            };
        var NotCompletedLegend = makeMyLegend("NotCompleted");
        NotCompletedLegend[1].Dom.innerHTML = "Not Completed";
        LegendContainer.Dom.appendChild(NotCompletedLegend[2].Dom);
        NotCompletedLegend[2].Dom.style.top = "66%";
        NotCompletedLegend[0].Dom.style.backgroundColor = "gray";


        var data = [CompletedTask, DisabledTask, NotCompleted];

        var option= {
        //Boolean - Whether we should show a stroke on each segment
        segmentShowStroke : true,
	
        //String - The colour of each segment stroke
        segmentStrokeColor : "rgba(50,50,50,.8)",
	
        //Number - The width of each segment stroke
        segmentStrokeWidth : 1,
	
        //The percentage of the chart that we cut out of the middle.
        percentageInnerCutout : 45,
	
        //Boolean - Whether we should animate the chart	
        animation : true,
	
        //Number - Amount of animation steps
        animationSteps : 100,
	
        //String - Animation easing effect
        animationEasing : "easeOutBounce",
	
        //Boolean - Whether we animate the rotation of the Doughnut
        animateRotate : true,

        //Boolean - Whether we animate scaling the Doughnut from the centre
        animateScale : false,
	
        //Function - Will fire on animation completion.
        onAnimationComplete : null
    }

        myNewChart.Doughnut(data, option);
        //pieChartContainer.Dom.style.height = "70px";
        //pieChartContainer.Dom.style.width = "140px";
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



    generateColorCircle.ID = 0;



    generateMyButton.ID = 0;
