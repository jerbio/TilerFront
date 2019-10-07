var TotalSubEventList = [];
var ActiveSubEvents = [];
var TwelveHourMilliseconds = OneHourInMs * 12;
var Dictionary_OfSubEvents = {};
var Dictionary_OfCalendarData = {};
var ToBeReorganized = [];
var global_DeltaSubevents = [];
var lowestMsToNow=999999999999999;
var ClosestSubEventToNow;
var global_RenderedList = [];
let currentSubevent
let nextSubEvent




function InitializeHomePage(DomContainer)
{
    initializeUserLocation();
    var verifiedUser = GetCookieValue();

    if (verifiedUser == "")
    {
        global_goToLoginPage();
    }

    //var myurl = "RootWagTap/time.top?WagCommand=0";
    var myurl = global_refTIlerUrl + "Schedule";
    ToBeReorganized = [];
    TotalSubEventList = new Array();
    ActiveSubEvents = new Array();
    TwelveHourMilliseconds = OneHourInMs * 12;
    Dictionary_OfSubEvents = {};
    ToBeReorganized = [];
    CurrentTheme.AppUIContainer = DomContainer;
    if (CurrentTheme.AppUIContainer == null)
    {
        CurrentTheme.AppUIContainer = document.getElementById("CalBodyContainer");
        $(CurrentTheme.AppUIContainer).empty();
    }
    $(CurrentTheme.AppUIContainer).empty();
    /*
    var UserIdenti = getGetParameter("User");
    var UserNumber = getGetParameter("UserNumber");
    */
    var occupy = "occupy";
    var preppePostdData = { UserName: verifiedUser.UserName, UserID: verifiedUser.UserID };
    retrieveUserSchedule(myurl, preppePostdData,generateCalendarEvents);
}

function ActiveRange()
{

}
ActiveRange.Start = new Date(new Date().getTime() - (OneHourInMs * 12));
ActiveRange.End = new Date(CurrentTheme.Now + TwelveHourMilliseconds);
ActiveRange.Refresh = function()
{
    ActiveRange.Start = new Date(new Date().getTime() - (OneHourInMs * 12));
    ActiveRange.End = new Date(CurrentTheme.Now + TwelveHourMilliseconds);
}

function getGetParameter(name) {
    if (name = (new RegExp('[?&]' + encodeURIComponent(name) + '=([^&]*)')).exec(location.search))
        return decodeURIComponent(name[1]);
}
function retrieveUserSchedule(myurl, UserEntry,SuccessCallBack)
{
    //$.get(myurl, generateCalendarEvents);
    //debugger;
    retrieveUserSchedule.callAllBeforeRefreshCallbacks();
    var TimeZone = new Date().getTimezoneOffset();
    UserCredentials.UserName= UserEntry.UserName;//, : ,TimeZoneOffset: TimeZone };
    UserCredentials.ID = UserEntry.UserID;
    UserCredentials.TimeZoneOffset = TimeZone;
    UserEntry.TimeZoneOffset = UserCredentials.TimeZoneOffset;
    UserEntry.StartRange = (new Date()).getTime() - TwelveHourMilliseconds;
    UserEntry.EndRange = (new Date()).getTime() + TwelveHourMilliseconds;
    UserEntry.TimeZone = moment.tz.guess()
    var HandleNewPage = new LoadingScreenControl("Tiler is retrieving your schedule :)");
    if(!!HandleNewPage.Launch){
        HandleNewPage.Launch();
    }
    
    $.ajax({
        type: "GET",
        url: myurl,
        data: UserEntry,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        //dataType: "json",
        success: function(response) {
            SuccessCallBack(response)
            retrieveUserSchedule.callAllCallbacks(response)
        },
        error: function (err) {
            var myError = err;
            var step = "err";
        }

    }).done(function (data) {
        if(!!HandleNewPage.Hide){    
            HandleNewPage.Hide();
        }
        var a = 1;
    });
}
retrieveUserSchedule.beforeRefreshCallbacks = {}
retrieveUserSchedule.callbacks = {}

retrieveUserSchedule.subscribeToBeforeRefresh = function (callbackFunction) {
    if ((!retrieveUserSchedule.beforeRefreshCallbacks)) {
        retrieveUserSchedule.beforeRefreshCallbacks = {}
    }
    retrieveUserSchedule.beforeRefreshCallbacks[Math.random().toString(36).substring(9)] = callbackFunction;
}

retrieveUserSchedule.subscribeToSuccessfulRefresh = function (callbackFunction) {
    if ((!retrieveUserSchedule.callbacks))
    {
        retrieveUserSchedule.callbacks = {}
    }
    retrieveUserSchedule.callbacks[Math.random().toString(36).substring(9)] = callbackFunction;
}

retrieveUserSchedule.callAllBeforeRefreshCallbacks = function (data) {
    var keys = Object.keys(retrieveUserSchedule.beforeRefreshCallbacks);
    for (var index in keys) {
        var callback = retrieveUserSchedule.beforeRefreshCallbacks[keys[index]];
        callback(data)
    }
}

retrieveUserSchedule.callAllCallbacks = function (data) {
    for (var index in Object.keys(retrieveUserSchedule.callbacks))
    {
        var callback = retrieveUserSchedule.callbacks[Object.keys(retrieveUserSchedule.callbacks)[index]];
        callback(data)
    }
}



function generateCalendarEvents(data) {
    //alert(typeof (data));
    //var JsonData = JSON.parse(data);
    var JsonData = (data);
    resetEventStatusUi()
    generateLoggedInUserAccountUI(JsonData.Content);
    //return JsonData;
}

function generateLoggedInUserAccountUI(UserSchedule) {
    generateHomePage(UserSchedule);
}

function generateHomePage(UserSchedule) {
    var CalendarBodyDom = CurrentTheme.AppUIContainer;
    var HomePageContainer = getHomePageDomContainer();
    //UserCredentials.Name = UserSchedule.Name;
    $(HomePageContainer.Dom).addClass("ScreenContainer");
    CurrentTheme.TransitionNewContainer(HomePageContainer.Dom);
    $(HomePageContainer.Dom).css({ "left": "0%" });
    $(HomePageContainer.Dom).empty();
    //CalendarBodyDom.appendChild(HomePageContainer.Dom);
    CurrentTheme.TransitionNewContainer(HomePageContainer.Dom);
    var HomePageDoms = generateHomePageDoms(HomePageContainer.Dom);
    var MiddleContentDom = HomePageDoms[1].childNodes[1];//second index selection selects content section
    var StructuredData = StructuralizeNewData(UserSchedule)
    TotalSubEventList = StructuredData.TotalSubEventList;
    ActiveSubEvents = StructuredData.ActiveSubEvents;
    Dictionary_OfCalendarData = StructuredData.Dictionary_OfCalendarData;
    Dictionary_OfSubEvents = StructuredData.Dictionary_OfSubEvents;
    ActiveSubEvents = getEventsWithinRange(ActiveRange.Start, ActiveRange.End);
    if (ActiveSubEvents.length)
    {
        ClosestSubEventToNow = getClosestToNow(ActiveSubEvents, new Date());
    }
    else
    {
        ClosestSubEventToNow = getClosestToNow(TotalSubEventList, new Date());
    }

    ActiveSubEvents.forEach(function (myEvent) {
        var MobileDom = generateMobileDoms(myEvent);
        myEvent.Dom = MobileDom;
        processNowAndNextRendering(myEvent);
    });
    /*var AllNonrepeatingNonEvents = generateNonRepeatEvents(UserSchedule.Schedule.NonRepeatCalendarEvent);
    var AllRepeatEventDoms = generateRepeatEvents(UserSchedule.Schedule.RepeatCalendarEvent);*/
    InitializeMiddleDomUI(MiddleContentDom);
    
    if (ClosestSubEventToNow && ClosestSubEventToNow.Dom != null)
    {
        var position = $(ClosestSubEventToNow.Dom.Dom).position();
        setTimeout(function () {
            $(MiddleContentDom).scrollTop(position.top);
            //scroll(0, );
        }, 4000);
    }
    
}

function generateMenuContainer()
{
    var MenuContainer = getDomOrCreateNew("MenuContainer");
    var MenuContent = generateMenuContent();
    MenuContainer.appendChild(MenuContent);
    MenuContainer.onclick = MenuToggle;
    var RetValue = MenuContainer;
    MenuContent.onclick = function (e) { e.stopPropagation() };
    $(MenuContent).on("swipe", MenuToggle)
    return RetValue;
}

function generateMenuContent()
{
    var MenuContainer = getDomOrCreateNew("MenuContent");

    var ManageButton = getDomOrCreateNew("ManageButton","button");
    var LogOutButton = getDomOrCreateNew("LogOutButton", "button");
    LogOutButton.innerHTML = "LogOut"
    ManageButton.innerHTML = "Manage"
    $(ManageButton).addClass("MenuItemButton")
    $(LogOutButton).addClass("MenuItemButton")
    MenuContainer.appendChild(ManageButton);
    MenuContainer.appendChild(LogOutButton);
    ManageButton.onclick = function () { window.location.href = ("../Manage") }
    LogOutButton.onclick = function () { document.getElementById('logoutForm').submit() }

    
    var RetValue = MenuContainer;
    return RetValue;
}

function generateHomePageDoms(Dom) {
    var TopBannerDom = document.getElementById("HomePageTopBanner");
    if (TopBannerDom == null) {
        TopBannerDom = document.createElement("div");
        TopBannerDom.setAttribute("id", "HomePageTopBanner");
        $(TopBannerDom).addClass(CurrentTheme.ContentSection);
        $(TopBannerDom).addClass(CurrentTheme.FontColor);
    }
    
    $(TopBannerDom).empty();
    TopBannerDom = populateHomePageTopBanner(TopBannerDom);

    var BodyDOM = document.getElementById("HomeMiddleContent");
    if (BodyDOM == null) {
        BodyDOM = document.createElement("div");
        BodyDOM.setAttribute("id", "HomeMiddleContent");
    }
    
    var MenuContainer = generateMenuContainer();
    $(Dom).append(BodyDOM);
    Dom.appendChild(MenuContainer);
    MenuContainer.onblur = MenuToggle;

    $(Dom).append(TopBannerDom);
    $(BodyDOM).empty();
    $(BodyDOM).addClass(CurrentTheme.ContentSection);
    BodyDOM = populateHomePageMiddleSection(BodyDOM);


    var DisablePanel = document.getElementById("DisablePanel");
    if (DisablePanel == null) {
        DisablePanel = document.createElement("div");
        DisablePanel.setAttribute("id", "DisablePanel");//sets the ID of Dom
        $(DisablePanel).addClass("DisablePanel");
    }
    $(DisablePanel).hide();
    $(BodyDOM).append(DisablePanel);

    var FooterDOM = document.getElementById("HomePageFooterDOM");

    
    if (FooterDOM == null) {
        FooterDOM = document.createElement("div");
        FooterDOM.setAttribute("id", "HomePageFooterDOM");
        $(FooterDOM).addClass("HomePageFooterOption");


    }

    $(Dom).append(FooterDOM);

    $(FooterDOM).empty();
    //FooterDOM.appendChild(HorizontalLine);
    FooterDOM = populateHomePageFooter(FooterDOM);
    $(FooterDOM).addClass(CurrentTheme.ContentSection);
    return [TopBannerDom, BodyDOM, FooterDOM];
}

function populateHomePageFooter(Dom)
{
    /*
    *Populates the bottom footer of a logged in User with the view buttons,and top divider
    */

    var AddButtonDom = document.getElementById("HomePageAddButton");
    if (AddButtonDom == null) {
        AddButtonDom = document.createElement("div");
        AddButtonDom.setAttribute("id", "HomePageAddButton");
        $(AddButtonDom).addClass(CurrentTheme.AddButton);
    }
    var ProcrastinateDom = document.getElementById("HomePageProcrastinate");
    if (ProcrastinateDom == null) {
        ProcrastinateDom = document.createElement("div");
        ProcrastinateDom.setAttribute("id", "HomePageProcrastinate");
        $(ProcrastinateDom).addClass(CurrentTheme.ProcrastinateAllIcon);
        ProcrastinateDom.addEventListener("click", onProcrastinateAll);
    }
    var shuffleButton = getDomOrCreateNew("ShuffleButton");
    $(shuffleButton).addClass("ShuffleButton ControlPanelButton SomethingNew")
    var shuffleCallback = function (response) {
        RefreshSubEventsMainDivSubEVents();
    }
    SomethingNewButton(shuffleButton, shuffleCallback);
    $(AddButtonDom).empty();
    $(ProcrastinateDom).empty();
    AddButtonDom.onclick = AddNewEventOnClick;

    $(Dom).append(AddButtonDom);
    $(Dom).append(shuffleButton);
    $(Dom).append(ProcrastinateDom);
    return Dom;
}





function generateProcrastinateAllFunction(TimeData,CallBack)
{

    var TimeZone = new Date().getTimezoneOffset();
    TimeData = TimeData.ToTimeSpan();
    var NowData = { DurationDays: TimeData.Days, DurationHours: TimeData.Hours, DurationMins: TimeData.Mins, UserName: UserCredentials.UserName, UserID: UserCredentials.ID, TimeZoneOffset: TimeZone };
    NowData.TimeZone = moment.tz.guess()
    var HandleNEwPage = new LoadingScreenControl("Tiler is Freeing up Some time :)");
    TimeData.Hours
    HandleNEwPage.Launch();
    var URL = global_refTIlerUrl + "Schedule/ProcrastinateAll";
    $.ajax({
        type: "POST",
        url: URL,
        data: NowData,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        success: function (response) {
            //alert(response);
            var myContainer = (response);
            if (myContainer.Error.code == 0) {
                //exitSelectedEventScreen();
            }
            else {
                alert("error detected with marking as complete");
            }

        },

        error: function (err) {
            var myError = err;
            var step = "err";
            var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
            var ExitAfter = { ExitNow: true, Delay: 1000 };
            HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, CallBack);
            //InitializeHomePage();


        }
    }).done(function (data) {
        HandleNEwPage.Hide();
        RefreshSubEventsMainDivSubEVents(CallBack);
        //InitializeHomePage();//hack alert
        //alert("alert 1-");
    });
}


function prepFunctionForCompletionOfEvent(EventID, CallBack) {
    return function () {

        var TimeZone = new Date().getTimezoneOffset();
        var Url;
        //Url="RootWagTap/time.top?WagCommand=7";
        Url = global_refTIlerUrl + "Schedule/Event/Complete";
        var HandleNEwPage = new LoadingScreenControl("Tiler is updating your schedule ...");
        HandleNEwPage.Launch();
        SubEvent = Dictionary_OfSubEvents[EventID]
        var MarkAsCompleteData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: EventID, TimeZoneOffset: TimeZone };

        MarkAsCompleteData = {
            UserName: UserCredentials.UserName,
            UserID: UserCredentials.ID,
            EventID: SubEvent.ID,
            TimeZoneOffset: TimeZone,
            ThirdPartyEventID: SubEvent.ThirdPartyEventID,
            ThirdPartyUserID: SubEvent.ThirdPartyUserID,
            ThirdPartyType: SubEvent.ThirdPartyType
        };
        MarkAsCompleteData.TimeZone = moment.tz.guess()
        $.ajax({
            type: "POST",
            url: Url,
            data: MarkAsCompleteData,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: function (response) {
                //alert(response);
                var myContainer = (response);
                if (myContainer.Error.code == 0) {
                    //exitSelectedEventScreen();
                }
                else {
                    alert("error detected with marking as complete");
                }

            },
            error: function (err) {
                var myError = err;
                var step = "err";
                var NewMessage="Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                var ExitAfter={ExitNow:true,Delay:1000};
                HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, InitializeHomePage);
                //InitializeHomePage();

                
            }

        }).done(function (data) {
            HandleNEwPage.Hide();
            RefreshSubEventsMainDivSubEVents(CallBack);
            //InitializeHomePage();//hack alert
        });
    }
}

function onProcrastinateAll()
{
    var DialHolder = function () {
        this.holder = null;
    }

    DialHolder.holder = new Dial(0, 5, 12, 0, 0);
    DialOnEvent(null, generateProcrastinateAllFunction, DialHolder.holder);
}

function populateSearchOptionDom(Dom)
{
    var SearchIconID = "HomepageSearchIcon";
    var SearchIcon = getDomOrCreateNew(SearchIconID);
    $(SearchIcon.Dom).addClass(CurrentTheme.SearchIcon);
    $(SearchIcon.Dom).click([Dom], generateSearchBarContainer);
    //$(SearchIcon).click(getSearchOverlay);
    return SearchIcon;
}



function generateModalForTIleOrModal()
{
    var AddOption = getDomOrCreateNew("AddOption");
    $(AddOption).addClass("AddOptionHidden");
    $(AddOption).removeClass("AddOptionDisappear");
    var ButtonContainer = getDomOrCreateNew("AddOptionButtonContainer");
    

    var AddTIleButton = getDomOrCreateNew("AddTIleButton");
    AddTIleButton.innerHTML = "Add New Tile";
    AddTIleButton.onclick = function () { callAddNewEvent(true) };
    
    var AddEventButton = getDomOrCreateNew("AddEventButton");
    AddEventButton.onclick = function () { callAddNewEvent(false) };
    AddEventButton.innerHTML = "Add New Event";
    var CancelButton = getDomOrCreateNew("CancelButton");
    CancelButton.innerHTML = "Cancel";

    CancelButton.onclick = function () { $(AddOption).addClass("AddOptionHidden"); };

    ButtonContainer.appendChild(AddTIleButton);
    ButtonContainer.appendChild(AddEventButton);
    ButtonContainer.appendChild(CancelButton);

    $(AddEventButton).addClass("AddOptionButton")
    $(AddTIleButton).addClass("AddOptionButton")
    $(CancelButton).addClass("AddOptionButton")

    AddOption.appendChild(ButtonContainer);

    var ContentCOntainer = CurrentTheme.getCurrentContainer();
    ContentCOntainer.appendChild(AddOption);

    setTimeout(function () { $(AddOption).removeClass("AddOptionHidden"); });

    function callAddNewEvent(isTile)
    {
        
        LaunchAddnewEvent(null, UserCredentials, isTile);
        //$(AddOption).addClass("AddOptionDisappear");
        
        setTimeout(function () { CancelButton.onclick(); }, 300);
        
    }
}

    function AddNewEventOnClick()
    {
        generateModalForTIleOrModal();
    }

    function generateMenuButton()
    {
        var ButtonID = "MenuButton"
        var MenuButton = getDomOrCreateNew(ButtonID);
        var ButtonIconBarAID = "IconBarA"
        var ButtonIconBarA = getDomOrCreateNew(ButtonIconBarAID);
        var ButtonIconBarBID = "IconBarB"
        var ButtonIconBarB = getDomOrCreateNew(ButtonIconBarBID);
        var ButtonIconBarCID = "IconBarC"
        var ButtonIconBarC = getDomOrCreateNew(ButtonIconBarCID);

        var IconBarContainer = getDomOrCreateNew("MenuIconBarContainer");
        IconBarContainer.appendChild(ButtonIconBarA);
        IconBarContainer.appendChild(ButtonIconBarB);
        IconBarContainer.appendChild(ButtonIconBarC);

        
        $(ButtonIconBarA).addClass("MenuIconBar")
        $(ButtonIconBarB).addClass("MenuIconBar")
        $(ButtonIconBarC).addClass("MenuIconBar")
        MenuButton.appendChild(IconBarContainer);
        MenuButton.onclick = MenuToggle;
        return MenuButton;
    }


    function MenuToggle()
    {
        var menuContainer = getDomOrCreateNew("MenuContainer");
        if (MenuToggle.isOn)
        {
            MenuToggle.isOn = false;
            menuContainer.style.left = "-100%";
        }
        else
        {
            MenuToggle.isOn = true;
            menuContainer.style.left = 0;
        }
    }
    MenuToggle.isOn = false;

    function populateHomePageTopBanner(Dom) {

        var FullInfoPanelID = "FullInfoPanel"
        var FullInfoPanel = getDomOrCreateNew(FullInfoPanelID);

        var TopBannerDisablePanelID = "TopBannerDisablePanel"
        var TopBannerDisablePanel = getDomOrCreateNew(TopBannerDisablePanelID);
        $(TopBannerDisablePanel.Dom).addClass("TopBannerDisablePanel")
        $(TopBannerDisablePanel.Dom).hide();
        Dom.appendChild(TopBannerDisablePanel.Dom);


        var LogOutButtonID = "LogOutButton"
        var LogOutButton = getDomOrCreateNew(LogOutButtonID, "button");
        LogOutButton.Dom.innerHTML = "LogOut";
        $(LogOutButton.Dom).addClass(CurrentTheme.BorderColor);
        $(LogOutButton.Dom).addClass(CurrentTheme.FontColor);
        //$(LogOutButton.Dom).click(function () {
        (LogOutButton.Dom).onclick=(function () {
            //LogOut();
            //LogOutButton.submit();
        });
        /*
        var LogOutContainer = getDomOrCreateNew("logoutForm");
        LogOutContainer.appendChild(LogOutButton.Dom)
        Dom.appendChild(LogOutContainer.Dom)
        */
        var MenuButton = generateMenuButton();
        Dom.appendChild(MenuButton);

        var LoggedLogoContainerID = "LoggedLogoContainer";
        var LoggedLogoContainer = getDomOrCreateNew(LoggedLogoContainerID);
        var innerLogoContainerID = "innerLogoContainer";
        var innerLogoContainer = getDomOrCreateNew(innerLogoContainerID);

    
        

        $(innerLogoContainer.Dom).addClass("WagTapLogo");
        LoggedLogoContainer.Dom.appendChild(innerLogoContainer.Dom);

    


        var ProfileInfoContainerID = "ProfileInfoContainer";
        var ProfileInfoContainer = getDomOrCreateNew(ProfileInfoContainerID);



        var NameProfileInfoContainerID = "NameProfileInfoContainer";
        var NameProfileInfoContainer = getDomOrCreateNew(NameProfileInfoContainerID);
        var NameProfileInfoContainerContentID = "NameProfileInfoContainerContent";
        var NameProfileInfoContainerContent = getDomOrCreateNew(NameProfileInfoContainerContentID,"span");
        NameProfileInfoContainerContent.Dom.innerHTML = UserCredentials.Name ==null? "Hello":"Hello " + UserCredentials.Name + "!";
        NameProfileInfoContainer.Dom.appendChild(NameProfileInfoContainerContent.Dom);


        var DateProfileInfoContainerID = "DateProfileInfoContainer";
        var DateProfileInfoContainer = getDomOrCreateNew(DateProfileInfoContainerID);
        var DateProfileInfoContainerContentID = "DateProfileInfoContainerContent";
        var DateProfileInfoContainerContent = getDomOrCreateNew(DateProfileInfoContainerContentID, "span");
        var refDate = new Date(Date.now());
        var DayString = "It's " +
            /*
            getTimeStr(refDate) +
            ", " +
            */
            Months[refDate.getMonth()] + " " + refDate.getDate();
        DateProfileInfoContainerContent.Dom.innerHTML = DayString;
        DateProfileInfoContainer.Dom.appendChild(DateProfileInfoContainerContent.Dom);



        ProfileInfoContainer.Dom.appendChild(NameProfileInfoContainer.Dom);
        ProfileInfoContainer.Dom.appendChild(DateProfileInfoContainer.Dom);
        $(ProfileInfoContainer.Dom).addClass(CurrentTheme.FontColor);

    var SearchButton = populateSearchOptionDom(Dom);
    var SearchButtonContainerID = "SearchButtonContainer";
    var SearchButtonContainer = getDomOrCreateNew(SearchButtonContainerID);
    SearchButtonContainer.Dom.appendChild(SearchButton.Dom);



	FullInfoPanel.Dom.appendChild(LoggedLogoContainer.Dom);
	FullInfoPanel.Dom.appendChild(ProfileInfoContainer.Dom);
	Dom.appendChild(FullInfoPanel.Dom);
	Dom.appendChild(SearchButtonContainer.Dom);

    


	Dom.appendChild(FullInfoPanel.Dom);

        //$(FooterButtonContainerDom).append(TwelveHourButtonDom);
        //$(FooterButtonContainerDom).append(WeekButtonDOM);
        //$(FooterButtonContainerDom).append(MonthButtonDOM);
        return Dom;
    }

    function LogOut()
    {
        delete_cookie();
        window.location.href = ("/Account/LogOff")
    }

    function getTimeStr(dt) {
        //var dt = new Date();
        var d = dt.toLocaleDateString();
        var t = dt.toLocaleTimeString();
        t = t.replace(/\u200E/g, '');
        t = t.replace(/^([^\d]*\d{1,2}:\d{1,2}):\d{1,2}([^\d]*)$/, '$1$2');
        var result = t;// + ', on ' + d;
        return result;
    }


    function populateHomePageMiddleSection(Dom) {
        var TopDivider = document.getElementById("HomePageTopDivider");
        if (TopDivider == null) {
            TopDivider = getNewDividerDom();
            TopDivider.setAttribute("id", "HomePageTopDivider");//sets the ID of Dom
        }
        var ContentSection = document.getElementById("HomePageMiddle");
    
        if (ContentSection == null) {
            ContentSection = document.createElement("div");
            ContentSection.setAttribute("id", "HomePageMiddle");//sets the ID of Dom

        }

        ContentSection.ontouchmove = function (e) { e.stopPropagation() };
        $(TopDivider).css({ "position": "relative",  "height": "6px", "width": "70%", "left": "15%", "box-shadow": "rgba(10,10,10,.9) 0px 5px 20px" });
        $(TopDivider).addClass(CurrentTheme.LineColor);
        $(ContentSection).addClass(CurrentTheme.RadialBackGround);
        //$(ContentSection).css({ "top": "20px" });



        $(Dom).append(TopDivider);
    
        $(Dom).append(ContentSection);
        return Dom;
    }

    function ActivateEventDom(EventDom)
    {
        EventDom.Dom.style.left = "15%";
    }

    


    function generateMobileDoms(myEvent) {
        Tiers = myEvent.Tiers;
        var EventDom = getDomOrCreateNew("EventID" + myEvent.ID);
        var HorizontalLine = InsertHorizontalLine("70%", "15%", "100%")//creates underlying gray Line
        HorizontalLine.style.zIndex = 3;
        //HorizontalLine.style.backgroundColor = "gray";
        //HorizontalLine.style.marginTop = "-6px";
        $(HorizontalLine).addClass("UnderLyingGrayLine");
        EventDom.Dom.appendChild(HorizontalLine);

        var EventDom_ContainerA = getDomOrCreateNew("EventID_ContainerA" + myEvent.ID);
        var EventDom_ContainerB = getDomOrCreateNew("EventID_ContainerB" + myEvent.ID);
        var EventDom_ContainerC = getDomOrCreateNew("SliderBand" + myEvent.ID);

        $(EventDom_ContainerA.Dom).addClass("EventDomContainerA");
        EventDom.Dom.appendChild(EventDom_ContainerA.Dom);



        EventDom.Dom.appendChild(EventDom_ContainerB.Dom);
        $(EventDom_ContainerB.Dom).addClass("EventDomContainerB");
        //$(EventDom_ContainerB.Dom).addClass(CurrentTheme.AlternateFontColor);


        $(EventDom_ContainerC.Dom).addClass("SliderBand");
        //$(EventDom_ContainerC.Dom).addClass(CurrentTheme.ContentSection);
        $(EventDom_ContainerC.Dom).addClass(CurrentTheme.BorderColor);

        var Diamond = getDomOrCreateNew("Diamond" + myEvent.ID);
        EventDom_ContainerC.Dom.appendChild(Diamond.Dom);
        $(Diamond.Dom).addClass("Diamond");
        $(Diamond.Dom).addClass(CurrentTheme.Border);
        $(Diamond.Dom).addClass(CurrentTheme.BorderColor);
        $(Diamond.Dom).addClass("LeftArrow");
        Diamond.status = 0;
        //"EventDomContainer"
        "EventDomContainerA"
        "EventDomContainerB"
        EventDom_ContainerC.Dom.onclick = generateFunctionForSliderClick()
        //$(EventDom_ContainerC.Dom).click(generateFunctionForSliderClick());
        var deleteCallbackFunction = genFunctionCallForDeletion(myEvent.ID, EventDom_ContainerB.Dom, triggerClickOfEventDom_ContainerCWhenDisablePanelIsClicked);
        var markAsCompleteCallBackFunction = prepFunctionForCompletionOfEvent(myEvent.ID, triggerClickOfEventDom_ContainerCWhenDisablePanelIsClicked);

        function triggerClickOfEventDom_ContainerCWhenDisablePanelIsClicked() {
            $(EventDom_ContainerC.Dom).trigger("click");
            var DisablePanel = document.getElementById("DisablePanel");
            DisablePanel.removeEventListener("click", triggerClickOfEventDom_ContainerCWhenDisablePanelIsClicked);
        }

        function generateFunctionForSliderClick() {
            return function () {
                var DisablePanel = document.getElementById("DisablePanel");
                var Complete_ProcrastinateAllIcon = document.getElementById("HomePageProcrastinate");
                if (Diamond.status) {
                    $(Diamond.Dom).removeClass("RightArrow");
                    $(Diamond.Dom).addClass("LeftArrow");
                    $(EventDom_ContainerA.Dom).show();
                    EventDom_ContainerC.Dom.style.left = "100%";
                    EventDom_ContainerB.Dom.style.left = "100%";
                    var addButton = document.getElementById("HomePageAddButton")
                    $(addButton).addClass("rotateToAdd");
                    $(addButton).removeClass("rotateToDelete");

                    //addButton.removeEventListener("click", deleteCallbackFunction);
                    //addButton.addEventListener("click", AddNewEventOnClick);
                    addButton.onclick = AddNewEventOnClick;
                    $(DisablePanel).hide();
                    DisablePanel.style.zIndex = 0;
                    EventDom.Dom.style.zIndex = 0;
                    Complete_ProcrastinateAllIcon.removeEventListener("click", markAsCompleteCallBackFunction);
                    Complete_ProcrastinateAllIcon.addEventListener("click", onProcrastinateAll);

                    $(Complete_ProcrastinateAllIcon).addClass(CurrentTheme.ProcrastinateAllIcon);
                    $(Complete_ProcrastinateAllIcon).removeClass(CurrentTheme.CompleteIcon);
                    DisablePanel.removeEventListener("click", triggerClickOfEventDom_ContainerCWhenDisablePanelIsClicked);
                }
                else {
                    $(Diamond.Dom).addClass("RightArrow");
                    $(Diamond.Dom).removeClass("LeftArrow");
                    //EventDom_ContainerC.Dom.style.marginLeft = 0;
                    EventDom_ContainerC.Dom.style.left = "40px";
                    $(EventDom_ContainerA.Dom).hide();
                    EventDom_ContainerB.Dom.style.left = "15%";

                    var addButton = document.getElementById("HomePageAddButton");

                    //addButton.removeEventListener("click", AddNewEventOnClick);
                    //addButton.addEventListener("click", deleteCallbackFunction);

                    addButton.onclick = deleteCallbackFunction;

                    Complete_ProcrastinateAllIcon.removeEventListener("click", onProcrastinateAll);
                    Complete_ProcrastinateAllIcon.addEventListener("click", markAsCompleteCallBackFunction);

                    $(addButton).addClass("rotateToDelete");
                    $(addButton).removeClass("rotateToAdd");
                    $(Complete_ProcrastinateAllIcon).addClass(CurrentTheme.CompleteIcon);
                    $(Complete_ProcrastinateAllIcon).removeClass(CurrentTheme.ProcrastinateAllIcon);
                    EventDom.Dom.style.zIndex = 1;
                    //DisablePanel.style.zIndex = 10;
                    $(DisablePanel).show();
                    DisablePanel.addEventListener("click", triggerClickOfEventDom_ContainerCWhenDisablePanelIsClicked);
                }

                Diamond.status += 1;
                Diamond.status %= 2;
            }
        }

        //var refStart = getHourMin(myEvent.SubCalStartDate);
        //var refEnd = getHourMin(myEvent.SubCalEndDate);

        //myEvent.SubCalStartDate.setHours(refStart.Hour, refStart.Minute);
        //myEvent.SubCalEndDate.setHours(refEnd.Hour, refEnd.Minute);

        var toString = myEvent.SubCalStartDate.toLocaleString();


        $(EventDom.Dom).addClass("EventDomContainer");
        $(EventDom.Dom).addClass(CurrentTheme.FontColor);
        $(EventDom_ContainerA.Dom).addClass("EventDomContainerA");
        if (myEvent.color == null) {
            $(EventDom_ContainerA.Dom).addClass(CurrentTheme.DefaultEventColor);
            //$(EventDom_ContainerB.Dom).addClass(CurrentTheme.DefaultEventColor);
        }

        var EventDescription = getDomOrCreateNew("EventDescription" + myEvent.ID);
        $(EventDom_ContainerA.Dom).append(EventDescription.Dom);
        $(EventDescription.Dom).addClass("EventDescriptionContainer");
        if (myEvent.ColorSelection > 0) {
            $(EventDescription.Dom).addClass(global_AllColorClasses[myEvent.ColorSelection].cssClass);
        }



        //Top Description. Shows Name of Event
        var EventTopDescription = getDomOrCreateNew("EventTopDescription" + myEvent.ID);
        var EventTopDescriptionText = getDomOrCreateNew("EventTopDescriptionText" + myEvent.ID);
        $(EventDescription.Dom).append(EventTopDescription.Dom);
        $(EventTopDescription.Dom).addClass("EventTopDescription");
        //EventTopDescription.Dom.innerHTML = "<p>" + myEvent.Name + "</P>";

        EventTopDescriptionText.Dom.innerHTML = myEvent.Name;
        $(EventTopDescriptionText.Dom).addClass("EventTopDescriptionText");
        EventTopDescription.Dom.appendChild(EventTopDescriptionText.Dom);
        //var HorizontalLine = InsertHorizontalLine("90%", "10%", "50%","3px",true);
        //HorizontalLine.style.zIndex = "10";
        //EventDescription.Dom.appendChild(HorizontalLine);




        //Bottom Description. Shows time and Location
        var EventBottomDescription = getDomOrCreateNew("EventBottomDescription" + myEvent.ID);
        $(EventDescription.Dom).append(EventBottomDescription.Dom);
        $(EventBottomDescription.Dom).addClass("EventBottomDescription");
        //shows line

        //(PercentHeight, LeftPosition, TopPosition, thickness, Alternate) 
        //var BottoDescVerticalLine = InsertVerticalLine("80%", "50%", "10%", "3px", true);
        //$(EventBottomDescription.Dom).append(BottoDescVerticalLine);

        //Bottom Description. Shows Location
        var EventBottomLeftDescription = getDomOrCreateNew("EventBottomLeftDescription" + myEvent.ID);
        var EventBottomLeftDescriptionTextContainer = getDomOrCreateNew("EventBottomLeftDescriptionTextContainer" + myEvent.ID);
        var EventBottomLeftDescriptionText = getDomOrCreateNew("EventBottomLeftDescriptionText" + myEvent.ID);

        //$(EventBottomLeftDescriptionTextContainer.Dom).addClass("EventGenericDescriptionTextContainer");
        $(EventBottomLeftDescriptionTextContainer.Dom).addClass("EventGenericDescriptionTextContainer");
        $(EventBottomLeftDescriptionText.Dom).addClass("EventGenericDescriptionTextContainer");//enables ellipsis
        $(EventBottomLeftDescriptionTextContainer.Dom).append(EventBottomLeftDescriptionText.Dom);



        $(EventBottomDescription.Dom).append(EventBottomLeftDescription.Dom);
        $(EventBottomLeftDescription.Dom).append(EventBottomLeftDescriptionTextContainer.Dom);
        $(EventBottomLeftDescription.Dom).addClass("EventBottomLeftDescription");

        //$(EventBottomLeftDescriptionText.Dom).addClass("EventBottomGenericDescriptionText");
        if (myEvent.SubCalAddressDescription) {
            EventBottomLeftDescriptionText.Dom.innerHTML = "@ " + myEvent.SubCalAddressDescription;
        }


        //Bottom Description. Container for Traffic Light and What is left
        var EventBottomRightDescription = getDomOrCreateNew("EventBottomRightDescription" + myEvent.ID);
        var EventBottomRightDescriptionTextContainer = getDomOrCreateNew("EventBottomRightDescriptionTextContainer" + myEvent.ID);
        var EventBottomRightDescriptionText = getDomOrCreateNew("EventBottomRightDescriptionText" + myEvent.ID);
        var EventBottomRightTrafficContainer = getDomOrCreateNew("EventBottomRightTrafficContainer" + myEvent.ID);
        $(EventBottomRightTrafficContainer.Dom).addClass("EventBottomRightTrafficContainer");
        EventBottomRightDescription.Dom.appendChild(EventBottomRightTrafficContainer.Dom);

        $(EventBottomRightDescriptionTextContainer.Dom).addClass("EventGenericDescriptionTextContainer");
        $(EventBottomRightDescriptionTextContainer.Dom).append(EventBottomRightDescriptionText.Dom);


        if (myEvent.ID == "151_209_210") {
            var qq = 45;
        }

        var alertLevel = getSubEventAlertLevel(myEvent.SubCalStartDate, Tiers);
        var TrafficLightData = generateTrafficLightContainer(myEvent.ID, alertLevel);
        EventBottomRightTrafficContainer.Dom.appendChild(TrafficLightData.Container.Dom)

        $(EventBottomDescription.Dom).append(EventBottomRightDescription.Dom);
        $(EventBottomRightDescription.Dom).append(EventBottomRightDescriptionTextContainer.Dom);
        $(EventBottomRightDescription.Dom).addClass("EventBottomRightDescription");




        //EventBottomRightDescriptionText.Dom.innerHTML = "@ " + myEvent.SubCalAddressDescription;





        //shows Current Time
        var EventTopRightDescription = getDomOrCreateNew("EventTopRightDescription" + myEvent.ID);
        var EventTopRightDescriptionTextContainer = getDomOrCreateNew("EventTopRightDescriptionTextContainer" + myEvent.ID);
        var EventTopRightDescriptionText = getDomOrCreateNew("EventTopRightDescriptionText" + myEvent.ID);

        //$(EventTopRightDescriptionTextContainer.Dom).addClass("EventGenericDescriptionTextContainer");
        $(EventTopRightDescriptionTextContainer.Dom).append(EventTopRightDescriptionText.Dom);



        $(EventTopDescription.Dom).append(EventTopRightDescription.Dom);
        $(EventTopRightDescription.Dom).append(EventTopRightDescriptionTextContainer.Dom);
        $(EventTopRightDescription.Dom).addClass("EventTopRightDescription");


        //$(EventTopRightDescriptionText.Dom).addClass("EventBottomGenericDescriptionText");
        EventTopRightDescriptionText.Dom.innerHTML = getTimeStringFromDate(myEvent.SubCalStartDate);// + "-" + getTimeStringFromDate(myEvent.SubCalEndDate);



        //Procrastinate Button
        var EventProcrastinateContainer = getDomOrCreateNew("EventProcrastinateContainer" + myEvent.ID);
        var EventProcrastinateButton = getDomOrCreateNew("EventProcrastinateButton" + myEvent.ID);
        var EventProcrastinateImageContainer = getDomOrCreateNew("EventProcrastinateImageContainer" + myEvent.ID);
        var EventProcrastinateTextContainer = getDomOrCreateNew("EventProcrastinateTextContainer" + myEvent.ID);

        //$(EventProcrastinateContainer.Dom).click(function () {
        (EventProcrastinateContainer.Dom).onclick = (function () {
            var myEventID = myEvent.ID;
            ProcrastinateOnEvent(myEventID, EventDom_ContainerA.Dom, function () {
                triggerClickOfEventDom_ContainerCWhenDisablePanelIsClicked();
            });
        });

        EventDescription.Dom.onclick = function () {
            var myEventID = myEvent.ID;

            LaunchSubEventSelection(myEventID, myEvent);
        };
        /*$(EventDescription.Dom).click(function () {
            var myEventID = myEvent.ID;

            LaunchSubEventSelection(myEventID, myEvent);
        });*/

        var EventDom_ContainerB_ElementContainer = getDomOrCreateNew("EventDom_ContainerB_ElementContainer" + myEvent.ID);
        $(EventDom_ContainerB_ElementContainer.Dom).addClass("EventDom_ContainerB_ElementContainer");
        $(EventDom_ContainerB.Dom).append(EventDom_ContainerB_ElementContainer.Dom);


        $(EventDom_ContainerB_ElementContainer.Dom).append(EventProcrastinateContainer.Dom);
        $(EventProcrastinateContainer.Dom).append(EventProcrastinateButton.Dom);



        $(EventProcrastinateContainer.Dom).addClass("EventProcrastinateContainer");
        $(EventProcrastinateContainer.Dom).addClass("ContainerB_Element");
        var VerticalLine = InsertVerticalLine("65%", "0%", "17.5%", "3px", true);
        //EventProcrastinateContainer.Dom.appendChild(VerticalLine);
        $(EventProcrastinateButton.Dom).addClass("EventProcrastinateButton");
        $(EventProcrastinateImageContainer.Dom).addClass("ProcrastinateIcon");
        $(EventProcrastinateImageContainer.Dom).addClass("ContainerB_ElementImage");
        $(EventProcrastinateTextContainer.Dom).addClass("ContainerB_ElementText");
        EventProcrastinateTextContainer.Dom.innerHTML = "Procrastinate";
        EventProcrastinateButton.Dom.appendChild(EventProcrastinateImageContainer.Dom);
        EventProcrastinateButton.Dom.appendChild(EventProcrastinateTextContainer.Dom);

        //$(EventProcrastinateButton.Dom).addClass("ProcrastinateIcon");






        //Directions Button
        var EventDirectionsContainer = getDomOrCreateNew("EventDirectionsContainer" + myEvent.ID);
        $(EventDirectionsContainer.Dom).addClass("ContainerB_Element");
        VerticalLine = InsertVerticalLine("65%", "0%", "17.5%", "3px", true);
        //EventDirectionsContainer.Dom.appendChild(VerticalLine);

        var EventDirectionsButton = getDomOrCreateNew("EventDirectionsButton" + myEvent.ID);
        var EventDirectionsImageContainer = getDomOrCreateNew("EventDirectionsImageContainer" + myEvent.ID);
        var EventDirectionsTextContainer = getDomOrCreateNew("EventDirectionsTextContainer" + myEvent.ID);
        EventDirectionsButton.Dom.appendChild(EventDirectionsImageContainer.Dom);
        EventDirectionsButton.Dom.appendChild(EventDirectionsTextContainer.Dom);

        $(EventDirectionsImageContainer.Dom).addClass("DirectionsIcon");
        $(EventDirectionsImageContainer.Dom).addClass("ContainerB_ElementImage");
        $(EventDirectionsTextContainer.Dom).addClass("ContainerB_ElementText");
        EventDirectionsTextContainer.Dom.innerHTML = "Directions";


        //$(EventDom_ContainerB.Dom).append(EventDirectionsContainer.Dom);

        $(EventDom_ContainerB_ElementContainer.Dom).append(EventDirectionsContainer.Dom);
        $(EventDirectionsContainer.Dom).append(EventDirectionsButton.Dom);

        $(EventDirectionsContainer.Dom).addClass("EventDirectionsContainer");
        $(EventDirectionsButton.Dom).addClass("EventDirectionsButton");
        //$(EventDirectionsButton.Dom).addClass("DirectionsIcon");
        //$(EventDirectionsContainer.Dom).click(getDirectionsCallBack(myEvent.ID, CurrentTheme));
        (EventDirectionsContainer.Dom).onclick = (getDirectionsCallBack(myEvent.ID, CurrentTheme));



        //Now Button
        var EventNowContainer = getDomOrCreateNew("EventNowContainer" + myEvent.ID);
        $(EventNowContainer.Dom).addClass("ContainerB_Element");
        VerticalLine = InsertVerticalLine("65%", "0%", "17.5%", "3px", true);
        //EventNowContainer.Dom.appendChild(VerticalLine);
        var EventNowButton = getDomOrCreateNew("EventNowButton" + myEvent.ID);
        var EventNowImageContainer = getDomOrCreateNew("EventNowImageContainer" + myEvent.ID);
        var EventNowTextContainer = getDomOrCreateNew("EventNowTextContainer" + myEvent.ID);
        EventNowButton.Dom.appendChild(EventNowImageContainer.Dom);
        EventNowButton.Dom.appendChild(EventNowTextContainer.Dom);

        $(EventNowImageContainer.Dom).addClass("NowIcon");
        $(EventNowImageContainer.Dom).addClass("ContainerB_ElementImage");
        $(EventNowTextContainer.Dom).addClass("ContainerB_ElementText");
        EventNowTextContainer.Dom.innerHTML = "Do Now!";


        //$(EventDom_ContainerB.Dom).append(EventDirectionsContainer.Dom);
        $(EventDirectionsContainer.Dom).append(EventDirectionsButton.Dom);



        //$(EventDom_ContainerB.Dom).append(EventNowContainer.Dom);
        $(EventDom_ContainerB_ElementContainer.Dom).append(EventNowContainer.Dom);
        $(EventNowContainer.Dom).append(EventNowButton.Dom);

        $(EventNowContainer.Dom).addClass("EventNowContainer");
        $(EventNowButton.Dom).addClass("EventNowButton");
        //$(EventNowButton.Dom).addClass("NowIcon");
        //$(EventNowContainer.Dom).click(genFunctionCallForNow(myEvent.ID, EventDom_ContainerB.Dom, triggerClickOfEventDom_ContainerCWhenDisablePanelIsClicked));
        (EventNowContainer.Dom).onclick = (genFunctionCallForNow(myEvent.ID, EventDom_ContainerB.Dom, triggerClickOfEventDom_ContainerCWhenDisablePanelIsClicked));


        //Delete Button
        var EventDeleteContainer = getDomOrCreateNew("EventDeleteContainer" + myEvent.ID);
        $(EventDeleteContainer.Dom).addClass("ContainerB_Element");
        VerticalLine = InsertVerticalLine("65%", "0%", "17.5%", "3px", true);
        EventDeleteContainer.Dom.appendChild(VerticalLine);
        var EventDeleteButton = getDomOrCreateNew("EventDeleteButton" + myEvent.ID);
        //$(EventDom_ContainerB.Dom).append(EventDeleteContainer.Dom);
        $(EventDeleteContainer.Dom).append(EventDeleteButton.Dom);
        //$(EventDeleteContainer.Dom).click(genFunctionCallForDeletion(myEvent.ID, EventDom_ContainerB.Dom));
        (EventDeleteContainer.Dom).onclick = (genFunctionCallForDeletion(myEvent.ID, EventDom_ContainerB.Dom));



        //PopulateRigid Lock
        var EventLockContainer = getDomOrCreateNew("EventLockContainer" + myEvent.ID);
        $(EventLockContainer.Dom).addClass("EventLockContainer");
        var myBool = (myEvent.SubCalRigid)
        var EventLockImgContainer = getDomOrCreateNew("EventLockImgContainer" + myEvent.ID);
        $(EventLockImgContainer.Dom).addClass("EventLockImgContainer");

        /*
        EventLockImgContainer.Dom.style.height = "100px";
        EventLockImgContainer.Dom.style.top = "50%"
        EventLockImgContainer.Dom.style.marginTop = "-50px"
        EventLockImgContainer.Dom.style.width = "70px";
        EventLockImgContainer.Dom.style.left = "50%"
        EventLockImgContainer.Dom.style.marginLeft = "-35px"*/

        if (myBool) {
            $(EventLockImgContainer.Dom).addClass("LockedIcon");
        }


        EventLockContainer.Dom.appendChild(EventLockImgContainer.Dom)
        EventDom.Dom.appendChild(EventLockContainer.Dom);
        EventDom.Dom.appendChild(EventDom_ContainerC.Dom);





        $(EventDeleteContainer.Dom).addClass("EventDeleteContainer");
        $(EventDeleteButton.Dom).addClass("EventDeleteButton");
        $(EventDeleteButton.Dom).addClass("DeleteIcon");

        return EventDom;
    }

    function getSubEventAlertLevel(StartTime, AllTiers)
    {
        StartTime = new Date(StartTime);

        var TestTIme = new Date(2014, 6, 2, 8, 0, 0, 0);
        var TestTIme_Number = TestTIme.getTime();

        var Tier0 = new Date(AllTiers[0]*1000);
        var Tier1 = new Date(AllTiers[1] * 1000);
        var Tier2 = new Date(AllTiers[2] * 1000);
        if (StartTime.getTime() < Tier0.getTime())
        {
            return 0;
        }
    
        if (StartTime.getTime() < Tier1.getTime())
        {
            return 1;
        }

        return 2;    
    }

    function generateTrafficLightContainer(ID,Level)
    {
        var EventTrafficLightContainer = getDomOrCreateNew("EventTrafficLightContainer" + ID);
        $(EventTrafficLightContainer.Dom).addClass("EventTrafficLightContainer");

        var AllBulbs = new Array();

        var GreenLightContainer = getDomOrCreateNew("GreenLightContainer" + ID);
        $(GreenLightContainer.Dom).addClass("GreenLightContainer");
        $(GreenLightContainer.Dom).addClass("TrafficLightContainer");
        var GreenLightBulbContainer = getDomOrCreateNew("GreenLightBulbContainer " + ID);
        $(GreenLightBulbContainer.Dom).addClass("GreenLightBulbContainer");
        $(GreenLightBulbContainer.Dom).addClass("TrafficLightBulb");
        GreenLightContainer.Dom.appendChild (GreenLightBulbContainer.Dom)


        AllBulbs.push(GreenLightBulbContainer);



        var OrangeLightContainer = getDomOrCreateNew("OrangeLightContainer" + ID);
        $(OrangeLightContainer.Dom).addClass("OrangeLightContainer");
        $(OrangeLightContainer.Dom).addClass("TrafficLightContainer");
        var OrangeLightBulbContainer = getDomOrCreateNew("OrangeLightBulbContainer " + ID);
        $(OrangeLightBulbContainer.Dom).addClass("OrangeLightBulbContainer");
        $(OrangeLightBulbContainer.Dom).addClass("TrafficLightBulb");
        OrangeLightContainer.Dom.appendChild(OrangeLightBulbContainer.Dom)

        AllBulbs.push(OrangeLightBulbContainer);

        var RedLightContainer = getDomOrCreateNew("RedLightContainer" + ID);
        $(RedLightContainer.Dom).addClass("RedLightContainer");
        $(RedLightContainer.Dom).addClass("TrafficLightContainer");
        var RedLightBulbContainer = getDomOrCreateNew("RedLightBulbContainer " + ID);
        $(RedLightBulbContainer.Dom).addClass("RedLightBulbContainer");
        $(RedLightBulbContainer.Dom).addClass("TrafficLightBulb");
        RedLightContainer.Dom.appendChild(RedLightBulbContainer.Dom)


    

    

    
        AllBulbs.push(RedLightBulbContainer);

        EventTrafficLightContainer.Dom.appendChild(GreenLightContainer.Dom);
        EventTrafficLightContainer.Dom.appendChild(OrangeLightContainer.Dom);
        EventTrafficLightContainer.Dom.appendChild(RedLightContainer.Dom);

        var i = 0;
        for(;i<AllBulbs.length;i++)
        {
            if(i!=Level)
            {
                //var DarkTrafficBulb = getDomOrCreateNew("DarkTrafficBulb_" + ID+"_"+i);
                $(AllBulbs[i].Dom).addClass("DarkTrafficBulb");
                //$(DarkTrafficBulb.Dom).addClass("TrafficLightBulb");
                //$(AllBulbs[i].Dom).append(DarkTrafficBulb.Dom);
            }
        }

        var retValue = {Container:EventTrafficLightContainer, AllBulbs: AllBulbs }
        return retValue;
    }


    function ProcrastinateOnEvent(EventID, ParentDom,CallBack)
    {
        //$(ParentDom).empty();
        var EventProcrastinateButtonContainer;// = document.getElementById("EventProcrastinateButtonContainer" + EventID);
        //ParentDom.removeChild(EventProcrastinateButtonContainer);
        //EventProcrastinateButtonContainer.removeNode(true);
        //var CopyOfProcrastination=$(EventProcrastinateButtonContainer).clone();
        //$(CurrentTheme.AppUIContainer).children().hide();
        ParentDom=CurrentTheme.AppUIContainer
  
        var EventProcrastinateContainer = getDomOrCreateNew("EventProcrastinateContainerSelected" + EventID);
    
        $(EventProcrastinateContainer.Dom).addClass("ProcrastinateContainer");    
        $(EventProcrastinateContainer.Dom).addClass(CurrentTheme.ContentSection);

        EventProcrastinateButtonContainer = document.createElement("div");
        EventProcrastinateButtonContainer.setAttribute("id", "EventProcrastinateButtonContainerSelected" + EventID);
        $(EventProcrastinateButtonContainer).addClass("SelectedProcrastinateButtonContainer");

        EventProcrastinateContainer.Dom.appendChild(EventProcrastinateButtonContainer);


        //$(EventProcrastinateButtonContainer).css({ "left": "0%", "height": "10%", "width": "100%", "z-index": "10" });
    
        var EventBackButton = document.createElement("div");
        var EventUpdateButton = document.createElement("div");
        var ActiveEventProcratination = document.createElement("div");



        //$(ActiveEventProcratination).css({ "background-color:": "rgba(10,10,10,.8)" });
        EventProcrastinateButtonContainer.appendChild(EventBackButton);

        var closeProcrastinateContainer = function () {
            var myEventProcrastinateButtonContainer = getDomOrCreateNew("EventProcrastinateContainerSelected" + EventID);
            //alert("empty....");
            //myEventProcrastinateButtonContainer.innerHTML = "";
            $(myEventProcrastinateButtonContainer).empty();
            myEventProcrastinateButtonContainer.outerHTML = "";
            CurrentTheme.TransitionOldContainer();

            //$(ParentDom).children().show();

        }
        EventBackButton.onclick =closeProcrastinateContainer;        

        //$(EventUpdateButton).click
            EventUpdateButton.onclick=(function () {
            closeProcrastinateContainer();
            var TimeData = CurrentDial.ToTimeSpan();

            var TimeZone = new Date().getTimezoneOffset();
            var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: EventID, DurationDays: TimeData.Days, DurationHours: TimeData.Hours, DurationMins: TimeData.Mins ,TimeZoneOffset: TimeZone };
            NowData.TimeZone = moment.tz.guess()
            var URL = global_refTIlerUrl + "Schedule/Event/Procrastinate";
            var HandleNEwPage = new LoadingScreenControl("Tiler is Postponing  :)");
            HandleNEwPage.Launch();
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
                    //alert("alert 0-");
                },
                error: function ()
                {
                    var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                    var ExitAfter = { ExitNow: true, Delay: 1000 };
                    HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, InitializeHomePage);
                }
            }).done(function (data) {
                HandleNEwPage.Hide()
                RefreshSubEventsMainDivSubEVents(CallBack);
                //InitializeHomePage();//hack alert
            });

            return;

        });


        var ProcrastinateInputContainer = getDomOrCreateNew("ProcrastinateInputContainer" + EventID)
        $(ProcrastinateInputContainer.Dom).addClass("ProcrastinateInputContainer");

        EventProcrastinateButtonContainer.appendChild(EventUpdateButton);
        EventProcrastinateButtonContainer.appendChild(ActiveEventProcratination);


        $(EventProcrastinateButtonContainer).addClass(CurrentTheme.DefaultEventColor);

        $(EventBackButton).addClass("BackIcon");
        $(EventBackButton).css({ "position":"absolute","left": "1.5%", "width": "50px", "height": "50px", "top": "50%", "margin-top": "-25px" });

        $(EventUpdateButton).addClass("CheckIcon");
        $(EventUpdateButton).css({ "position":"absolute","left": "98.5%", "width": "50px", "height": "50px", "margin-left": "-50px", "top": "50%", "margin-top": "-25px" });



        $(ActiveEventProcratination).addClass("ActiveEventProcrastination");
        var CurrentDial, SelectedProcrastinateOptionDomButton;

        var DayTextBox = getDomOrCreateNew("ProcrastinateDayInput" + EventID);
        DayTextBox.Dom.innerHTML= "<p>Day(s)</p>";
        DayTextBox.Dom.setAttribute("class", "ProcrastinateInput ProcrastinateDayInput");
        //$(DayTextBox.Dom).click(function ()
        (DayTextBox.Dom).onclick=(function ()
        {
            //var CurrentDayValue = new Dial(0, 1, 24, "Day(s)", "Hour(s)", 24 * 2, 0);
            var CurrentDayValue = new Dial(0, 1, 1, 24 * 2);
            CurrentDial = CurrentDayValue;
            $(SelectedProcrastinateOptionDomButton.Dom).removeClass("SelectedProcrastinateButton");
            SelectedProcrastinateOptionDomButton = DayTextBox;
        
            $(SelectedProcrastinateOptionDomButton.Dom).addClass("SelectedProcrastinateButton");
            InitializeProcrastinateDial(CurrentDayValue, EventProcrastinateContainer.Dom);
        })


        var HourTextBox = getDomOrCreateNew("ProcrastinateHourInput" + EventID);
        HourTextBox.Dom.setAttribute("class", "ProcrastinateInput ProcrastinateHourInput");
        HourTextBox.Dom.innerHTML = "<p>Hour(s)</p>";
        //$(HourTextBox.Dom).click(function () {
        (HourTextBox.Dom).onclick=(function () {
            //var CurrentHourValue = new Dial(0, 5, 60, "Hour(s)", "min(s)", 60 * 2, 1);
            var CurrentHourValue = new Dial(0, 5,0, 60*2);
            CurrentDial = CurrentHourValue;
            $(SelectedProcrastinateOptionDomButton.Dom).removeClass("SelectedProcrastinateButton");
            SelectedProcrastinateOptionDomButton = HourTextBox;
            $(SelectedProcrastinateOptionDomButton.Dom).addClass("SelectedProcrastinateButton");
            InitializeProcrastinateDial(CurrentHourValue, EventProcrastinateContainer.Dom);
        })

        var MinTextBox = getDomOrCreateNew("ProcrastinateMinInput" + EventID);;
        MinTextBox.Dom.setAttribute("class", "ProcrastinateInput ProcrastinateMinInput");
        MinTextBox.Dom.innerHTML = "<p>Min(s)</p>";

    
    
        function InitializeProcrastinateDial(SelectedDial, ParentContainer)
        {
            var KnobHolder = getDomOrCreateNew("KnobContainer" + EventID);
            $(KnobHolder.Dom).empty();
            
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

            var KnobJS = getDomOrCreateNew("KnobJS_ext" + EventID);
            $(KnobJS.Dom).addClass("KnobJS");
            KnobJS.Dom.setAttribute("data-bgColor", "rgb(80,80,80)");
            //KnobJS.Dom.setAttribute("data-displayInput", "true");





            //KnobJS.Dom.setAttribute("data-fgColor", "#66CC66");

            $(KnobHolder.Dom).addClass(CurrentTheme.ContentSection);
            var CircleDialCOntainer = getDomOrCreateNew("CircleDialCOntainer")
            CircleDialCOntainer.Dom.appendChild(KnobJS.Dom);
            //KnobHolder.Dom.appendChild()
            KnobHolder.Dom.appendChild(CircleDialCOntainer.Dom);
            ParentContainer.appendChild(KnobHolder.Dom);






            $(KnobHolder.Dom).addClass(CurrentTheme.ContentSection);
            var CircleDialCOntainer = getDomOrCreateNew("CircleDialCOntainer")
            CircleDialCOntainer.Dom.appendChild(KnobJS.Dom);
            //KnobHolder.Dom.appendChild()
            KnobHolder.Dom.appendChild(CircleDialCOntainer.Dom);
            ParentContainer.appendChild(KnobHolder.Dom);


            $(KnobHolder.Dom).addClass(CurrentTheme.ContentSection);
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
                var changeInValueFunc = function (meDial) {
                    incr(meDial.newVal);
                }

                var mydial = new Y.Dial({
                    min: 0,
                    max: 100000,
                    stepsPerRevolution: SelectedDial.getFullRevolution(),
                    value: 0,
                    diameter: 300,
                    minorStep: 1,
                    majorStep: 5,
                    decimalPlaces: 0,
                    strings: { label: 'Altitude in Kilometers:', resetStr: 'Reset', tooltipHandle: 'Drag to set' },
                    // construction-time event subscription
                    after: {
                        valueChange: Y.bind(changeInValueFunc, mydial)
                    }
                });
                mydial.render(KnobJS.Dom);
            });

        }
    

        ProcrastinateInputContainer.Dom.appendChild(DayTextBox.Dom);
        ProcrastinateInputContainer.Dom.appendChild(HourTextBox.Dom);
        ProcrastinateInputContainer.Dom.appendChild(MinTextBox.Dom);
        ActiveEventProcratination.appendChild(ProcrastinateInputContainer.Dom);
        CurrentDial = new Dial(0, 5, 0, 60 * 2); SelectedProcrastinateOptionDomButton = HourTextBox;

        $(SelectedProcrastinateOptionDomButton.Dom).addClass("SelectedProcrastinateButton");

        CurrentTheme.TransitionNewContainer(EventProcrastinateContainer.Dom);

        InitializeProcrastinateDial(CurrentDial, EventProcrastinateContainer.Dom);

    

        /*$(ParentDom).children().hide();
        $(ParentDom).append(EventProcrastinateContainer.Dom);*/
    }


    function genFunctionCallForNow(EventID,ParentDom,CallBack)
    {
        return function ()
        {
            var TimeZone = new Date().getTimezoneOffset();
            var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: EventID, TimeZoneOffset: TimeZone };
            //var URL = "RootWagTap/time.top?WagCommand=8"
            var URL = myurl = global_refTIlerUrl + "Schedule/Event/Now";
            NowData.TimeZone = moment.tz.guess()
            var HandleNEwPage = new LoadingScreenControl("Tiler is moving up your Event ...  :)");
            HandleNEwPage.Launch();
            

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
                RefreshSubEventsMainDivSubEVents(CallBack);
                //InitializeHomePage();//hack alert
            });
        }
    }



    function genFunctionCallForDeletion(EventID, ParentDom,CallBack)
    {
        function retValue() {
            var deletionConfirmationDom = getDomOrCreateNew("deletionConfirmationDom" + EventID);
            var deletionConfirmationDomWarning = getDomOrCreateNew("deletionConfirmationDomWarning" + EventID);
            $(deletionConfirmationDomWarning.Dom).addClass("deletionConfirmationDomWarning");
            deletionConfirmationDomWarning.Dom.innerHTML = "Permanently Delete?"


            $(deletionConfirmationDom.Dom).addClass("deletionConfirmationDom");
            $(deletionConfirmationDom.Dom).addClass(CurrentTheme.AlternateContentSection);

            $(deletionConfirmationDomWarning.Dom).addClass(CurrentTheme.ContentSection);
            $(deletionConfirmationDomWarning.Dom).addClass(CurrentTheme.FontColor);

            deletionConfirmationDom.Dom.appendChild(deletionConfirmationDomWarning.Dom);


            var deletionConfirmationDomButtonContainer = getDomOrCreateNew("deletionConfirmationDomButtonContainer" + EventID);
            $(deletionConfirmationDomButtonContainer.Dom).addClass("deletionConfirmationDomButtonContainer");
            deletionConfirmationDom.Dom.appendChild(deletionConfirmationDomButtonContainer.Dom);

            var deletionConfirmationDomYes = getDomOrCreateNew("deletionConfirmationDomYes" + EventID);
            $(deletionConfirmationDomYes.Dom).addClass("deletionConfirmationDomYes");
            //$(deletionConfirmationDomYes.Dom).click(generateFunctionForYes(EventID, CallBack));
            (deletionConfirmationDomYes.Dom).onclick=(generateFunctionForYes(EventID, CallBack));

        


            deletionConfirmationDomYes.Dom.innerHTML = "Yes";


            var deletionConfirmationDomNo = getDomOrCreateNew("deletionConfirmationDomNo" + EventID);
            $(deletionConfirmationDomNo.Dom).addClass("deletionConfirmationDomNo");
            //$(deletionConfirmationDomNo.Dom).click(generateFunctionForNo(deletionConfirmationDom.Dom));
            (deletionConfirmationDomNo.Dom).onclick=(generateFunctionForNo(deletionConfirmationDom.Dom));
            deletionConfirmationDomNo.Dom.innerHTML = "No";



            $(deletionConfirmationDomYes.Dom).addClass(CurrentTheme.ContentSection);
            $(deletionConfirmationDomYes.Dom).addClass(CurrentTheme.FontColor);
            $(deletionConfirmationDomNo.Dom).addClass(CurrentTheme.ContentSection);
            $(deletionConfirmationDomNo.Dom).addClass(CurrentTheme.FontColor);


            deletionConfirmationDomButtonContainer.Dom.appendChild(deletionConfirmationDomYes.Dom);
            deletionConfirmationDomButtonContainer.Dom.appendChild(deletionConfirmationDomNo.Dom);
            ParentDom.appendChild(deletionConfirmationDom.Dom);

        
        }

        return retValue;
    }

    function generateFunctionForYes(EventID, CallBack)
    {
        return function()
        {
            var TimeZone = new Date().getTimezoneOffset();
            var SubEvent = Dictionary_OfSubEvents[EventID]
            var DeletionEvent = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: EventID, TimeZoneOffset: TimeZone };
            DeletionEvent = {
                UserName: UserCredentials.UserName,
                UserID: UserCredentials.ID,
                EventID: SubEvent.ID,
                TimeZoneOffset: TimeZone,
                ThirdPartyEventID: SubEvent.ThirdPartyEventID,
                ThirdPartyUserID: SubEvent.ThirdPartyUserID,
                ThirdPartyType: SubEvent.ThirdPartyType
            };
            //v
            //var URL = "RootWagTap/time.top?WagCommand=6"
            var URL = global_refTIlerUrl + "Schedule/Event";
            var HandleNEwPage = new LoadingScreenControl("Tiler is Deleting your event :)");
            HandleNEwPage.Launch();

            $.ajax({
                type: "DELETE",
                url: URL,
                data: DeletionEvent,
                // DO NOT SET CONTENT TYPE to json
                // contentType: "application/json; charset=utf-8", 
                // DataType needs to stay, otherwise the response object
                // will be treated as a single string
                success: function (response) {
                    //InitializeHomePage();
                    //alert("alert 0-b");
                },
                error:function()
                {
                    var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                    var ExitAfter = { ExitNow: true, Delay: 1000 };
                    HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, InitializeHomePage);
                }
            }).done(function (data) {
                RefreshSubEventsMainDivSubEVents(CallBack);
                //InitializeHomePage();//hack alert
            });
        }
    }


    function generateFunctionForNo(ParentDom) {
        return function () {
            ParentDom.outerHTML = "";
        }
    }


    function processNowAndNextRendering(SubCalEvent) {
        let now = Date.now();
        let isCurrentSubEvent = now < SubCalEvent.SubCalEndDate.getTime() && now >= SubCalEvent.SubCalStartDate.getTime();
        if(isCurrentSubEvent && !currentSubevent) {
            currentSubevent = SubCalEvent;
            renderNowUi(SubCalEvent);
        }
        if(!currentSubevent && !nextSubEvent) {
            let isNext = now < SubCalEvent.SubCalStartDate.getTime();
            if(isNext) {
                nextSubEvent = SubCalEvent;
                renderNextUi(SubCalEvent);
            }
        }
    }


    function InitializeMiddleDomUI(Dom)
    {
        var CurrentTopPercent = 0;
        var TopIncrement = 0;
        let now = Date.now()
        ActiveSubEvents.sort(function (a, b) { return (a.SubCalStartDate) - (b.SubCalStartDate) });
        /*AllNonRepeatingEvents.forEach(
            function (CalendarEvent)
            {
                CalendarEvent.AllSubCalEvents.forEach(
                    function (SubCalEvent)
                    {
                        if (ActiveSubEvents.indexOf(SubCalEvent) > -1)
                        {
                            Dom.appendChild(SubCalEvent.Dom.Dom);
                            SubCalEvent.Dom.Dom.style.top = "100%";
                            
                            var myFunc = generateAFunction(CurrentTopPercent, SubCalEvent.Dom.Dom);
    
                            deferredCall(CurrentTopPercent*10, myFunc);
                            CurrentTopPercent += TopIncrement;
                        }
                    });
            });*/
        ActiveSubEvents.forEach
        (
            function (SubCalEvent)
            {
                Dom.appendChild(SubCalEvent.Dom.Dom);
                //SubCalEvent.Dom.Dom.style.top = "100%";
                var myFunc = generateAFunction(CurrentTopPercent, SubCalEvent.Dom.Dom);
                deferredCall(CurrentTopPercent * 1, myFunc);
                //CurrentTopPercent += TopIncrement;
            }
        )

        //global_RenderedList = ActiveSubEvents;
    }

    function renderNowUi (subEvent) {
        if(subEvent) {
            let currentSubEventClassName = "ListElementContainerCurrentSubevent";
            let ListElementContainer = subEvent.Dom
            $(ListElementContainer.Dom).addClass(currentSubEventClassName);
            let nextSubEventTimeSpanInMs =  subEvent.SubCalEndDate.getTime() - Date.now();
            let nextSubEventIndex = TotalSubEventList.indexOf(subEvent)
            if(nextSubEventIndex >=0 && nextSubEventIndex < TotalSubEventList.length - 1) {
                ++nextSubEventIndex;
                let nextSubEvent = TotalSubEventList[nextSubEventIndex]
                if(nextSubEventTimeSpanInMs >= OneMinInMs) {
                    setTimeout(() => {
                        $(ListElementContainer.Dom).removeClass(currentSubEventClassName);
                        renderNextUi(nextSubEvent);
                    }, nextSubEventTimeSpanInMs)
                } else {
                    $(ListElementContainer.Dom).removeClass(currentSubEventClassName);
                    renderNowUi(nextSubEvent);
                }
            }
        }
    }


    function renderNextUi(nextSubEvent) {
        if(nextSubEvent) {
            let nextSubEventClassName = "ListElementContainerNextSubevent";
            let ListElementContainer = nextSubEvent.Dom;
            $(ListElementContainer.Dom).addClass(nextSubEventClassName);
            let timeSpanInMs = nextSubEvent.SubCalStartDate.getTime() - Date.now()
            setTimeout(() => {
                renderNowUi(nextSubEvent)
                $(ListElementContainer.Dom).removeClass(nextSubEventClassName);
            }, timeSpanInMs)
        }
    }

    function RefreshSubEventsMainDivSubEVents(CallBack)
    {
        //debugger;
        var curentActiveElements = ActiveSubEvents;
        Dictionary_OfSubEvents = {};
        Dictionary_OfCalendarData = {};
        ActiveSubEvents = [];
        TotalSubEventList = [];
        sortOutData.OldActiveEvents = curentActiveElements;
        getNewData(sortOutData.OldActiveEvents);
        if (CallBack!=null)
        {
            CallBack();
        }
    }

    RefreshSubEventsMainDivSubEVents = buildFunctionSubscription(RefreshSubEventsMainDivSubEVents)

    function resetEventStatusUi() {
        if (currentSubevent) {
            let allCurrents = []
            let currentSubEventClassName = "ListElementContainerCurrentSubevent";
            let elements = $('.' + currentSubEventClassName)
            for (let i = 0; i < elements.length; i++) {
                let element = elements.get(i);
                $(element).removeClass(currentSubEventClassName);
            }
            currentSubevent= undefined;
        }
        if (nextSubEvent) {
            let nextSubEventClassName = "ListElementContainerNextSubevent";
            let elements = $('.' + nextSubEventClassName)
            for (let i = 0; i < elements.length; i++) {
                let element = elements.get(i);
                $(element).removeClass(nextSubEventClassName);
            }
            nextSubEvent= undefined;
        }
    }

    resetEventStatusUi = buildFunctionSubscription(resetEventStatusUi)
    
    RefreshSubEventsMainDivSubEVents.enroll(resetEventStatusUi, true);

    function getNewData(OldActiveEvents)
    {
        var myurl = global_refTIlerUrl + "Schedule";
        var verifiedUser = GetCookieValue();
        var preppePostdData = { UserName: verifiedUser.UserName, UserID: verifiedUser.UserID };
        retrieveUserSchedule(myurl, preppePostdData, sortOutData);
    }

    function sortOutData(PostData)
    {
        var UserSchedule = PostData.Content;
        var StructuredData = StructuralizeNewData(UserSchedule)
        TotalSubEventList = StructuredData.TotalSubEventList;
        ActiveSubEvents = StructuredData.ActiveSubEvents;
        Dictionary_OfCalendarData = StructuredData.Dictionary_OfCalendarData;
        Dictionary_OfSubEvents = StructuredData.Dictionary_OfSubEvents;
        ActiveSubEvents = getEventsWithinRange(ActiveRange.Start, ActiveRange.End);
        if (ActiveSubEvents.length) {
            ClosestSubEventToNow = getClosestToNow(ActiveSubEvents, new Date());
        }
        else {
            ClosestSubEventToNow = getClosestToNow(TotalSubEventList, new Date());
        }
        ActiveSubEvents.forEach(function (myEvent) {
            var MobileDom = generateMobileDoms(myEvent);
            myEvent.Dom = MobileDom;
            processNowAndNextRendering(myEvent);
        });
        /*
        var AllNonrepeatingNonEvents = generateNonRepeatEvents(UserSchedule.Schedule.NonRepeatCalendarEvent);
        var AllRepeatEventDoms = generateRepeatEvents(UserSchedule.Schedule.RepeatCalendarEvent);*/
        
        //adds all transition to all subcal elements
        for (var i = 0; i < sortOutData.OldActiveEvents.length; i++)
        {
            $(sortOutData.OldActiveEvents[i].Dom).addClass("EventDomContainerTransition");
        }


        //adds all transition to all subcal elements
        for (var i = 0; i < ActiveSubEvents.length; i++) {
            $(ActiveSubEvents[i].Dom).addClass("EventDomContainerTransition");
        }

        var ToBeDeletedDict = {};
        var JustNewEventsDict = {};
        
        var DictOfOldEvents = eventsToDict(sortOutData.OldActiveEvents);
        var DictOfNewEvents = eventsToDict(ActiveSubEvents);
        
        var DeleteAllLaterFuncs = [];
        populateNewEleemnts();
        deleteOldEvents();
        refreshOldEventsData();
        reOrderOldEvents();
        spliceInNewData();
        setTimeout(function ()
        {
            for (var i = 0; i < DeleteAllLaterFuncs.length; i++)
            {
                DeleteAllLaterFuncs[i]();
            }
        }, 3000);
        function eventsToDict(ArrayOfSubs)
        {
            var retValue = {};
            for (var i = 0 ;i< ArrayOfSubs.length; i++)
            {
                retValue[ArrayOfSubs[i].ID] = ArrayOfSubs[i];
            }
            return retValue;
        }
        function populateNewEleemnts()
        {
            for (var Id in DictOfNewEvents)
            {
                if (DictOfOldEvents[Id] == null)
                {
                    JustNewEventsDict[Id] = DictOfNewEvents[Id];
                }
            }
        }
        
        function deleteOldEvents()
        {
            var AllIDsToBeDeleted = []
            var i = 0;
            for (var Id in DictOfOldEvents)
            {
                ++i;

                //$(DictOfOldEvents[Id].Dom).removeClass("EventDomContainerBeneath");
                if (DictOfNewEvents[Id] == null)
                {
                    var toBeDeletedDom = DictOfOldEvents[Id].Dom;
                    HideHeight(toBeDeletedDom,i);
                    //debugger;
                    DeleteAllLaterFuncs.push(DeleteLater(toBeDeletedDom));
                    AllIDsToBeDeleted.push(Id);
                    ToBeDeletedDict[Id] = DictOfNewEvents[Id];
                }
            }

            function HideHeight(toBeDeletedDom, i)
            {
                setTimeout(function ()
                {
                    $(toBeDeletedDom.Dom).addClass("HideEventDescriptionContainer")
                }, (i * 0));
            }

            function DeleteLater(toBeDeletedDom)
            {
                return function ()
                {
                    $(toBeDeletedDom.Dom).removeClass("HideEventDescriptionContainer");
                    $(toBeDeletedDom.Dom).removeClass("RevealEventDescriptionContainer");
                    toBeDeletedDom.Dom.parentElement.removeChild(toBeDeletedDom.Dom);
                }
            }
            
            for (var i = 0; i < AllIDsToBeDeleted.length; i++)
            {
                delete DictOfOldEvents[AllIDsToBeDeleted[i]];
            }
            
        }

        function refreshOldEventsData()
        {
            for (var Id in DictOfOldEvents)
            {
                DictOfOldEvents[Id] = DictOfNewEvents[Id];
            }
        }

        function reOrderOldEvents()
        {
            var arrayOfOldEvents = [];
            for (var Id in DictOfOldEvents)
            {
                arrayOfOldEvents.push(DictOfOldEvents[Id]);
            }
            arrayOfOldEvents.sort(function (a, b) { return (a.SubCalStartDate) - (b.SubCalStartDate) });
            if (arrayOfOldEvents.length>0)
            {
                var nextElement = arrayOfOldEvents[0].Dom
                for (var i = 1 ; i < arrayOfOldEvents.length; i++)
                {
                    $(arrayOfOldEvents[i].Dom).insertAfter(nextElement.Dom);
                    nextElement = arrayOfOldEvents[i].Dom;
                }
                
            }

        }

        function spliceInNewData()
        {
            var arrayOfOldEvents = ActiveSubEvents;
            arrayOfOldEvents.sort(function (a, b) { return (a.SubCalStartDate) - (b.SubCalStartDate) });
            if (arrayOfOldEvents.length > 0)
            {
                var nextElement = arrayOfOldEvents[0].Dom
                for (var i = 1 ; i < arrayOfOldEvents.length; i++)
                {
                    deferredRevealCall(nextElement, i)
                    $(arrayOfOldEvents[i].Dom).insertAfter(nextElement.Dom);
                    nextElement = arrayOfOldEvents[i].Dom;
                }
                
            }

            function deferredRevealCall(nextElement, i)
            {
                setTimeout(function () { $(nextElement).addClass("RevealEventDescriptionContainer"); }, i * 200);
            }
        }

    }
    sortOutData.OldActiveEvents = [];

    function ResolveDelta(NewActiveSubevent)
    {

    }


    function UpdateRenderedList(AllSubEvents)
    {

    }
    function generateAFunction(vara, myDom)
    {

        var myFunc = function () {
            /*
            var myPercent = vara;
            //alert(myPercent);
            var stringDelay = myPercent + "px";
            //var myDom = SubCalEvent.Dom.Dom
            myDom.style.top = stringDelay;
            */
            $(myDom).addClass("RevealEventDescriptionContainer")
            addTransition(myDom);
        }
        return myFunc;
    }

    /*Funciton adds transition effects to subcalevents elements. Delays using timeout*/
    function addTransition(myDom)
    {
        setTimeout(function () { $(myDom).addClass("EventDomContainerTransition"); }, 1000);
    }
    function deferredCall(TimeOut, functionToCall)
    {
        setTimeout(functionToCall, TimeOut);
    }


    var weyy = 0;

    

    function LaunchSubEventSelection(EventID, SelectedEvent) {
        loadSelectedSubEvent(EventID, SelectedEvent);
        /*$.when(
        $.getScript("Scripts/SelectedEvent.js"),
        $.Deferred(function (deferred) {
            $(deferred.resolve);
        })).done(function () {
    
            //place your code here, the scripts are all loaded
            //alert("before other call "+CurrentTheme.getCurrentContainer().innerHTML);
            loadSelectedSubEvent(EventID, SelectedEvent);
        });*/


    }



    function getFullTimeFromEntry(Timpicker, DatePicker, ExtraDayInMS)//generateFunctionForFullTImeRetrieval
    {
        return function () {
            var TimePickerValue = Timpicker !=null?Timpicker.Dom.value:"12:00am";
            var DatePickerValue = DatePicker.Dom.value;
            let IsDefault = false;
            if (Timpicker.Dom.value.toLowerCase() == "")//handles initializing string values string 
            {
                TimePickerValue = getTimeStringFromDate(new Date(Date.now())).replace(" ", "");
                IsDefault = true;
            }

            if ((DatePicker.Dom.value.toLowerCase() == "") || (DatePicker.Dom.value.toLowerCase() == "")) {
                var DayPlusOne = Number(new Date(Date.now())) + ExtraDayInMS;
                {
                    DayPlusOne = new Date(DayPlusOne);
                    var Day = DayPlusOne.getDate();
                    var Month = DayPlusOne.getMonth() + 1;
                    var Year = DayPlusOne.getFullYear();
                    DatePickerValue = Month + "/" + Day + "/" + Year;
                }
            }


            var TwentyFourHourTime =AP_To24Hour(TimePickerValue);
            var DateData = date_mm_dd__yyyy_ToDateObj(DatePickerValue, "/")
            var retValue = { Time: TwentyFourHourTime, Date: DateData, IsDefault: IsDefault };

            return retValue;

        }
    }