var EventNameDom;
var EventAddressDom;
var EventAddressTagDom;
var EventRigid;
var EventStartDateTimeDom;
var EventEndDateTimeDom;
var EventSplits = 1;
var EventrepeatStatus;
var EventNonRigidDurationHolder
var EventRepeatStart;
var EventRepeatEnd;
var EventRepetitionSelection;
var EventColor;
var EventRestrictionFalseFlag;
var EventRestrictionStart;
var EventRestrictionEnd;
var EventRestrictionIsWorkWeek;



function LaunchAddnewEvent(LoopBackCaller, CurrentUser, isTIle) {
    EventNameDom = null;
    EventAddressDom = null;
    EventAddressTagDom = null;
    EventRigid = null;
    EventStartDom = null;
    EventSplits = 1;
    EventEndDom = null;
    EventrepeatStatus = null;
    EventNonRigidDurationHolder = null;
    EventRepeatStart = null;
    EventRepeatEnd = null;
    EventRepetitionSelection = null;


    var AddNewEventDomID = "AddNewEventContainer";
    var AddNewEvent = getDomOrCreateNew(AddNewEventDomID);
    var TabTitleContainerID = "TabTitleContainer";
    var TabTitleContainer = getDomOrCreateNew(TabTitleContainerID);
    var TabContentContainerID = "TabContentContainer";
    var TabContentContainer = getDomOrCreateNew(TabContentContainerID);
    $(TabTitleContainer.Dom).addClass(CurrentTheme.ContentSection);
    $(TabContentContainer.Dom).addClass(CurrentTheme.ContentSection);

    var AllTabs = new Array();


    var NameTab = createCalEventNameTab(isTIle);
    $(NameTab.Content.Dom).addClass(CurrentTheme.InActiveTabContent);
    $(NameTab.Title.Dom).addClass(CurrentTheme.AlternateFontColor);
    TabContentContainer.Dom.appendChild(NameTab.Content.Dom);
    TabTitleContainer.Dom.appendChild(NameTab.Title.Dom);

    AllTabs.push(NameTab);





    var RecurrenceTab = createCalEventRecurrenceTab(isTIle);
    $(RecurrenceTab.Content.Dom).addClass(CurrentTheme.InActiveTabContent);
    $(RecurrenceTab.Title.Dom).addClass(CurrentTheme.AlternateFontColor);
    TabContentContainer.Dom.appendChild(RecurrenceTab.Content.Dom);
    TabTitleContainer.Dom.appendChild(RecurrenceTab.Title.Dom);


    var CancelTab = createCalEventCancelTab();
    TabTitleContainer.Dom.appendChild(CancelTab.Button.Dom);
    var CloseAddNewEvent = function () {
        var myContainer = (CurrentTheme.getCurrentContainer());
        CurrentTheme.TransitionOldContainer();
        $(myContainer).empty();
        myContainer.outerHTML = "";
    }
    //$(CancelTab.Button.Dom).click(CloseAddNewEvent)
    //(CancelTab.Button.Dom).onclick = (CloseAddNewEvent)


    AllTabs.push(RecurrenceTab);

    var DoneTab = createCalEventDoneTab();
    //$(RangeTab.Content.Dom).addClass(CurrentTheme.InActiveTabContent);
    //TabContentContainer.Dom.appendChild(RangeTab.Content.Dom);
    //TabTitleContainer.Dom.appendChild(DoneTab.Button.Dom);
    var NewCalendarEvent;//
    //$(DoneTab.Button.Dom).click(function ()
    (DoneTab.Button.Dom).onclick = (function () {


        var NewEvent = prepCalDataForPost();

        if (NewEvent == null) {
            return;
        }
        NewEvent.UserName = CurrentUser.UserName
        NewEvent.UserID = CurrentUser.ID;

        var TimeZone = new Date().getTimezoneOffset();
        NewEvent.TimeZoneOffset = TimeZone;
        //var url = "RootWagTap/time.top?WagCommand=1"
        var url = global_refTIlerUrl + "Schedule/Event";

        var HandleNEwPage = new LoadingScreenControl("Tiler is Adding \"" + NewEvent.Name + " \" to your schedule ...");
        HandleNEwPage.Launch();


        $.ajax({
            type: "POST",
            url: url,
            data: NewEvent,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: function (response) {
                //alert(response);
                //debugger;
                //var myContainer = (CurrentTheme.getCurrentContainer());
                //CurrentTheme.TransitionOldContainer();
                //$(myContainer).empty();
                //myContainer.outerHTML = "";                
            },
            error: function (err) {
                //debugger;
                var myError = err;
                var step = "err";
                var NewMessage = "Oh No!!! Tiler is having issues modifying your schedule. Please try again Later :(";
                var ExitAfter = { ExitNow: true, Delay: 1000 };
                //HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, InitializeHomePage);
                HandleNEwPage.UpdateMessage(NewMessage, ExitAfter)
            }

        }).done(function (data) {
            debugger;
            /*var myContainer = (CurrentTheme.getCurrentContainer());
            CurrentTheme.TransitionOldContainer();
            $(myContainer).empty();
            myContainer.outerHTML = "";*/
            //InitializeHomePage();//hack alert
            RefreshSubEventsMainDivSubEVents(CloseAddNewEvent);

            var SubEventDate = new Date(data.Content.SubCalStartDate)
            var myDropDown = new DropDownNotification();
            myDropDown.ShowMessage("Earliest time for \"" + NewEvent.Name + "\" is " + SubEventDate.toLocaleString());
        });

        //alert(NewEvent);
        /*
        var repeteOpitonSelect = "none"
        if(EventrepeatStatus.status)
        {
            EventRepetitionSelection.forEach(function (Selection) { if (Selection.status) { repeteOpitonSelect = Selection } });
        }
        

        var ret = repeteOpitonSelect.Type.Name;

        alert(ret);*/
    }
);


    AddNewEvent.Dom.appendChild(TabContentContainer.Dom);
    AddNewEvent.Dom.appendChild(TabTitleContainer.Dom);
    (CancelTab.Button.Dom).onclick = (CloseAddNewEvent)
    TabTitleContainer.Dom.appendChild(DoneTab.Button.Dom);
    $(NameTab.Content.Dom).removeClass(CurrentTheme.InActiveTabContent);
    $(NameTab.Content.Dom).addClass(CurrentTheme.ActiveTabContent);
    ActivateTab(AllTabs, NameTab);
    if (LoopBackCaller == null) {
        CurrentTheme.TransitionNewContainer(AddNewEvent.Dom);
    }
    else {
        LoopBackCaller(AddNewEvent);
    }
}

function prepCalDataForPost() {
    var EventLocation = new Location(EventAddressTagDom.Dom.value, EventAddressDom.Dom.value);
    var EventName = EventNameDom.Dom.value;
    if (EventName == "") {
        alert("Please provide an Event Name");
        return null;
    }
    var Splits = EventSplits.Dom.value;
    //debugger;
    var CalendarColor = EventColor.getColor();
    CalendarColor = { r: CalendarColor.r, g: CalendarColor.g, b: CalendarColor.b, s: CalendarColor.Selection, o: CalendarColor.a };
    var EventDuration = EventNonRigidDurationHolder.holder.ToTimeSpan();
    var RigidFlag = !(EventRigid.status)
    var EventStart = EventStartDateTimeDom.getDateTimeData();

    var EventEnd = EventEndDateTimeDom.getDateTimeData();
    var DurationInMS = (EventDuration.Days * OneDayInMs) + (EventDuration.Hours * OneHourInMs) + (EventDuration.Mins * OneMinInMs)
    if (DurationInMS == 0) {
        alert("Oops please provide a duration for \"" + EventName + "\"");
        return null;
    }


    if (RigidFlag) {
        var TempEndDate = new Date(getFullTimeFromEntrytoJSDateobj(EventStart).getTime() + DurationInMS);
        EventEnd.Date = new Date(TempEndDate.getFullYear(), TempEndDate.getMonth(), TempEndDate.getDate());
        EventEnd.Time = { Hour: TempEndDate.getHours(), Minute: TempEndDate.getMinutes() };
    }

    var start = getFullTimeFromEntrytoJSDateobj(EventStart);
    var End = getFullTimeFromEntrytoJSDateobj(EventEnd);
    if (End <= start) {
        alert("End Time is earlier Than Start Time");
        return null;
    }

    var RepeatStatus = true;




    var DayPlusOne = new Date(CurrentTheme.Now);
    var Day = DayPlusOne.getDate();
    var Month = DayPlusOne.getMonth() + 1;
    var Year = DayPlusOne.getFullYear();
    var DatePickerValue = Month + "/" + Day + "/" + Year;


    var RepetitionStart = DatePickerValue;
    var RepetitionEnd = EventRepeatEnd.Dom.value;

    var repeteOpitonSelect = "none"
    if (EventrepeatStatus.status) {
        EventRepetitionSelection.forEach(function (Selection) { Selection.Dom = null; if (Selection.status) { repeteOpitonSelect = Selection } });
    }



    var EndDateTime = new Date();

    function generateRestrictionData() {
        var RestrictionStatusButtonStatus = !EventRestrictionFalseFlag.checked;
        var RestrictionStart = EventRestrictionStart.value;
        var RestrictionEnd = EventRestrictionEnd.value;
        var RestrictionWorkWeek = EventRestrictionIsWorkWeek.checked;
        var retValue = { isRestriction: RestrictionStatusButtonStatus, Start: RestrictionStart, End: RestrictionEnd, isWorkWeek: RestrictionWorkWeek }
        return retValue;
    }


    var RestrictionProfile = generateRestrictionData();

    debugger;
    var NewEvent = new CalEventData(EventName, EventLocation, Splits, CalendarColor, EventDuration, EventStart, EventEnd, repeteOpitonSelect, RepetitionStart, RepetitionEnd, RigidFlag, RestrictionProfile);
    debugger;
    NewEvent.RepeatData = null;
    return NewEvent;
}



function ActivateTab(AllTabs, ActiveTab) {
    var i = 0;

    //var retValue = { Title: NameTabTitle, Content: NameTabContent, Completion: CalEventIdentifierSectionCompletion };
    var j = i + 1;
    for (i; i < AllTabs.length; i++) {
        j = i + 1;
        $(AllTabs[i].Content.Dom).removeClass(CurrentTheme.ActiveTabContent);
        $(AllTabs[i].Title.Dom).removeClass(CurrentTheme.ActiveTabTitle);
        $(AllTabs[i].Title.Dom).removeClass(CurrentTheme.ContentSection);
        $(AllTabs[i].Title.Dom).addClass(CurrentTheme.AlternateContentSection);

        $(AllTabs[i].Content.Dom).addClass(CurrentTheme.InActiveTabContent);
        $(AllTabs[i].Title.Dom).addClass(CurrentTheme.InActiveTabTitle);
        $(AllTabs[i].Title.Dom).addClass(CurrentTheme.AlternateFontColor);
        AllTabs[i].DisableTab()
        $(AllTabs[i].Title.Dom).click(function (AllTabs, i) { return (function () { ActivateTab(AllTabs, AllTabs[i]) }) }(AllTabs, i));
        if (j < AllTabs.length) {
            $(AllTabs[i].Completion.Dom).click(function (AllTabs, i) { return (function () { ActivateTab(AllTabs, AllTabs[i]) }) }(AllTabs, j));
            AllTabs[i].Completion.Dom.innerHTML = "<p>Go To " + AllTabs[j].Name + "...</p>";
            $(AllTabs[i].Completion.Dom).addClass(CurrentTheme.AlternateFontColor);
        }
        else {
            $(AllTabs[i].Completion.Dom).click(function (AllTabs, i) { return (function () { ActivateTab(AllTabs, AllTabs[i]) }) }(AllTabs, 0));
            AllTabs[i].Completion.Dom.innerHTML = "<p>Go To " + AllTabs[0].Name + "...</p>";
            $(AllTabs[i].Completion.Dom).addClass(CurrentTheme.AlternateFontColor);
        }
    }

    ActiveTab.LaunchTab(AllTabs);
    $(ActiveTab.Content.Dom).addClass(CurrentTheme.ActiveTabContent);
    $(ActiveTab.Title.Dom).removeClass(CurrentTheme.AlternateFontColor);
    $(ActiveTab.Title.Dom).addClass(CurrentTheme.FontColor);
    $(ActiveTab.Title.Dom).removeClass(CurrentTheme.AlternateContentSection);
    $(ActiveTab.Title.Dom).addClass(CurrentTheme.ContentSection);

    $(ActiveTab.Title.Dom).removeClass(CurrentTheme.InActiveTabTitle);
    $(ActiveTab.Title.Dom).addClass(CurrentTheme.ActiveTabTitle);

}


function createCalEventNameTab(isTIle) {
    var NameTabContainerID = "NameTabContainer";
    var NameTabContainer = getDomOrCreateNew(NameTabContainerID);
    var NameTabTitleID = "NameTabTitle";
    var NameTabTitle = getDomOrCreateNew(NameTabTitleID);
    var NameTabTitleTextID = "NameTabTitleText";
    var NameTabTitleText = getDomOrCreateNew(NameTabTitleTextID);
    NameTabTitleText.Dom.innerHTML = "Add New Event";
    //NameTabTitleText.Dom
    var NameTabContentID = "NameTabContent";
    var NameTabContent = getDomOrCreateNew(NameTabContentID);
    NameTabTitle.Dom.appendChild(NameTabTitleText.Dom);
    $(NameTabTitleText.Dom).addClass("TabTitleText");
    $(NameTabTitle.Dom).addClass("TabTitle");
    $(NameTabTitle.Dom).addClass(CurrentTheme.AlternateFontColor);
    //InsertHorizontalLine(PercentWidth, LeftPosition, TopPosition, thickness, Alternate)
    var HorizontalLine = InsertHorizontalLine("100%", "00%", "100%", "4px");
    HorizontalLine.style.marginTop = "-4px";
    NameTabTitle.Dom.appendChild(HorizontalLine);
    $(NameTabContent.Dom).addClass("TabContent");
    $(NameTabContent.Dom).addClass(CurrentTheme.RadialBackGround);
    var test = { Name: "nskjfdnkf", ID: "fdjfdhjf", Default: "hfdjdhjks" };

    var CalEventNameParam = { Name: "Event Name", ID: "CalEventName", Default: "Event Name" };
    var CalEventAddressParam = { Name: "Address", ID: "CalEventAddress", Default: "Location" };
    var CalEventAddressTagParam = { Name: "Address NickName", ID: "CalEventAddressTag", Default: "Nickname?" };





    var CalEventNameInputDom = generateuserInput(CalEventNameParam, null);
    var CalEventNameInput = CalEventNameInputDom.FullContainer;
    CalEventNameInput.Dom.style.position = "relative";
    //CalEventNameInput.Dom.style.top = "0%";
    CalEventNameInput.Dom.style.width = "70%";
    //CalEventNameInput.Dom.style.height = "8%";
    //CalEventNameInputDom.Input.Dom.style.height = "16px";


    CalEventNameInput.Dom.style.left = "15%";
    $(CalEventNameInput.Dom).addClass(CurrentTheme.FontColor);
    $(CalEventNameInput.Dom).addClass("InputTextFont");
    //CalEventNameInputDom.Input.Dom.style.backgroundColor = "Transparent";
    CalEventNameInputDom.Input.Dom.style.border = "None"
    $(CalEventNameInputDom.Input.Dom).addClass(CurrentTheme.FontColor);
    //CalEventNameInputDom.Input.Dom.style.border = "solid 2px rgb(50,50,50)";

    EventNameDom = CalEventNameInputDom.Input;//public access of Name Dom
    var LabelContainerNameDom = CalEventNameInputDom.Label;
    EventNameDom.Dom.style.width = "100%";
    EventNameDom.Dom.style.fontFamily = "'Muli', sans-serif";


    EventNameDom.Dom.style.left = 0;
    LabelContainerNameDom.Dom.parentNode.removeChild(LabelContainerNameDom.Dom);

    var url = global_refTIlerUrl + "User/Location";
    var myAutoSuggestControl = new AutoSuggestControl(url, "GET", HandleLocationResult);


    function HandleLocationResult(data, DomContainer, InputContainer) {
        var EventIndex = 0;
        var HeightOfDom = 70;


        $(DomContainer.Dom).empty();
        (DomContainer.Dom).style.height = 0;
        if (data.length == 0 || data.length == null || data.length == undefined) {
            return;
        }

        data.forEach(resolveEachRetrievedEvent);

        function resolveEachRetrievedEvent(CalendarEvent) {
            var CalendarEventDom = generateDOM(CalendarEvent);
            CalendarEventDom.Dom.style.top = (EventIndex * HeightOfDom) + "px";
            CalendarEventDom.Dom.style.height = HeightOfDom + "px"
            DomContainer.Dom.appendChild(CalendarEventDom.Dom);
            DomContainer.Dom.style.height = (++EventIndex * HeightOfDom) + "px";

        }

        function generateDOM(LocationData) {
            var CacheLocationContainerID = "CacheLocationContainer" + EventIndex;
            var CacheLocationContainer = getDomOrCreateNew(CacheLocationContainerID)
            var CacheLocationContainerTagContainerID = "CacheLocationContainerTagContainer" + EventIndex;
            var CacheLocationContainerTagContainer = getDomOrCreateNew(CacheLocationContainerTagContainerID, "span");
            CacheLocationContainerTagContainer.Dom.innerHTML = LocationData.Tag + " - "
            $(CacheLocationContainerTagContainer.Dom).addClass("LocationTag");
            var CacheLocationAddressTagAddressID = "CacheLocationAddressTagAddress" + EventIndex;
            var CacheLocationAddressTagAddress = getDomOrCreateNew(CacheLocationAddressTagAddressID, "span");
            CacheLocationAddressTagAddress.Dom.innerHTML = LocationData.Address;
            $(CacheLocationAddressTagAddress.Dom).addClass("LocationAddress");
            $(CacheLocationContainer.Dom).click(OnClickOfDiv(LocationData, InputContainer));


            CacheLocationContainer.Dom.appendChild(CacheLocationContainerTagContainer.Dom);
            CacheLocationContainer.Dom.appendChild(CacheLocationAddressTagAddress.Dom);

            return CacheLocationContainer;
        }

        function OnClickOfDiv(LocationData, AddressInputDom) {
            return function () {
                var TagInputDom = CalEventAddressTagInputDom.Input.Dom;
                TagInputDom.value = LocationData.Tag;
                AddressInputDom.value = LocationData.Address;
                (DomContainer.Dom).style.height = 0;
                $(DomContainer.Dom).empty();
            }
        }
    }

    var CalEventAddressInputDom = generateuserInput(CalEventAddressParam, null);
    var CalEventAddressInput = CalEventAddressInputDom.FullContainer;
    //CalEventAddressInput.Dom.style.height = "8%";
    var LabelContainerAddressDom = CalEventAddressInputDom.Label;

    LabelContainerAddressDom.Dom.parentNode.removeChild(LabelContainerAddressDom.Dom);

    var CalEventAddressTagInputDom = generateuserInput(CalEventAddressTagParam, null);
    var CalEventAddressTagInput = CalEventAddressTagInputDom.FullContainer;
    //CalEventAddressTagInput.Dom.style.position = "absolute";
    CalEventAddressTagInput.Dom.style.position = "relative";
    //CalEventAddressTagInput.Dom.style.top = "16%";
    //CalEventAddressTagInput.Dom.style.marginTop = "6px";
    //CalEventAddressTagInput.Dom.style.height = "8%";

    CalEventAddressTagInput.Dom.style.width = "70%";
    CalEventAddressTagInput.Dom.style.left = "15%";
    $(CalEventAddressTagInput.Dom).addClass(CurrentTheme.FontColor);
    $(CalEventAddressTagInput.Dom).addClass("InputTextFont");
    //CalEventAddressTagInputDom.Input.Dom.style.backgroundColor = "Transparent";
    CalEventAddressTagInputDom.Input.Dom.style.border = 0
    $(CalEventAddressTagInputDom.Input.Dom).addClass(CurrentTheme.FontColor);
    //CalEventAddressTagInputDom.Input.Dom.style.border = "solid 2px rgb(50,50,50)";

    $(CalEventAddressInputDom.Input.Dom).remove();//deletes input Dom generated by generateuserInput()
    CalEventAddressInputDom.Input.Dom = myAutoSuggestControl.getAutoSuggestControlContainer();
    $(CalEventAddressInput.Dom).append(CalEventAddressInputDom.Input.Dom);
    CalEventAddressInputDom.Input.Dom.border = "none";
    var returnedCacheValuesContainer = myAutoSuggestControl.getSuggestedValueContainer();//gets autoSuggest container
    var AutoSuggestInputBar = myAutoSuggestControl.getInputBox();
    $(AutoSuggestInputBar).addClass(CurrentTheme.FontColor);
    $(returnedCacheValuesContainer).addClass(CurrentTheme.AlternateContentSection);
    $(returnedCacheValuesContainer).addClass(CurrentTheme.AlternateFontColor);
    returnedCacheValuesContainer.style.zIndex = 10;
    //AutoSuggestInputBar.style.backgroundColor = "Transparent";
    AutoSuggestInputBar.style.height = "100%";//height derived from  generateuserInput() function
    AutoSuggestInputBar.style.fontSize = "10px";
    AutoSuggestInputBar.style.border = "none";
    AutoSuggestInputBar.style.fontFamily = "'Muli', sans-serif";
    AutoSuggestInputBar.style.border = "none";
    AutoSuggestInputBar.style.borderBottom = "6px solid"
    AutoSuggestInputBar.setAttribute("placeholder", CalEventAddressParam.Default);

    $(AutoSuggestInputBar).addClass(CurrentTheme.BorderColor);



    //CalEventAddressInput.Dom.style.position = "absolute";
    //CalEventAddressInput.Dom.style.top = "8%";//based on dimensions in generateuserInput
    CalEventAddressInput.Dom.style.width = "70%";
    CalEventAddressInput.Dom.style.height = "8%";
    CalEventAddressInput.Dom.style.left = "15%";
    $(CalEventAddressInput.Dom).addClass(CurrentTheme.FontColor);
    $(CalEventAddressInput.Dom).addClass("InputTextFont");
    //CalEventAddressInputDom.Input.Dom.style.backgroundColor = "Transparent";
    CalEventAddressInputDom.Input.Dom.style.border = "None"
    $(CalEventAddressInputDom.Input.Dom).addClass(CurrentTheme.FontColor);
    //CalEventAddressInputDom.Input.Dom.style.borderBottom = "6px solid";
    //$(CalEventAddressInputDom.Input.Dom).addClass(CurrentTheme.BorderColor);
    EventAddressDom = CalEventAddressInputDom.Input;//public access of Address Dom
    EventAddressDom.Dom.style.width = "100%";
    EventAddressDom.Dom.style.left = 0;
    EventAddressDom.Dom.style.fontFamily = "'Muli', sans-serif";
    EventAddressDom.Dom = AutoSuggestInputBar;//hack alert sets the autosuggest bar as the  dom after setting all of the css styles


    EventAddressTagDom = CalEventAddressTagInputDom.Input;//public access of Address Tag Dom
    var LabelContainerAddressTagDom = CalEventAddressTagInputDom.Label;
    EventAddressTagDom.Dom.style.width = "100%";
    EventAddressTagDom.Dom.style.fontFamily = "'Muli', sans-serif";
    EventAddressTagDom.Dom.style.left = 0;
    //EventAddressTagDom.Dom.style.height = "100%";
    LabelContainerAddressTagDom.Dom.parentNode.removeChild(LabelContainerAddressTagDom.Dom);


    /* Auto Schedule Container start */
    var EnableAAutoScheduleContainerID = "EnableAAutoScheduleContainer";
    var EnableAAutoScheduleContainer = getDomOrCreateNew(EnableAAutoScheduleContainerID);

    var EnableAAutoScheduleContainerLabelID = "EnableAAutoScheduleContainerLabel";
    var EnableAAutoScheduleContainerLabel = getDomOrCreateNew(EnableAAutoScheduleContainerLabelID);
    EnableAAutoScheduleContainerLabel.Dom.innerHTML = "Unlock Tiler?";
    $(EnableAAutoScheduleContainerLabel.Dom).addClass(CurrentTheme.FontColor);
    var EnableAutoSchedulerButtonID = "EnableAutoSchedulerButton"
    var EnableAutoSchedulerButton = generateMyButton(AutoScheduleContainerLoopBack, EnableAutoSchedulerButtonID);
    $(EnableAutoSchedulerButton).addClass("setAsDisplayNone");


    //EnableAutoSchedulerButton.status = 0;



    EventRigid = EnableAutoSchedulerButton;

    EnableAAutoScheduleContainer.Dom.appendChild(EnableAAutoScheduleContainerLabel.Dom)


    //Split Input box

    /*
    var AutoScheduleCountContainerID = "AutoScheduleCountContainer";
    var AutoSchedulerDataInputData = { Name: "", ID: AutoScheduleCountContainerID, Default: "Splits?" };
    var AutoSchedulerDataInputDataCounterDom = generateuserInput(AutoSchedulerDataInputData, null);
    $(AutoSchedulerDataInputDataCounterDom.FullContainer.Dom).addClass(CurrentTheme.FontColor);
    AutoSchedulerDataInputDataCounterDom.FullContainer.Dom.style.borderBottom = "none";
    $(AutoSchedulerDataInputDataCounterDom.Input.Dom).addClass("InputBox");
    $(AutoSchedulerDataInputDataCounterDom.Label.Dom).addClass("InputLabel");
    $(AutoSchedulerDataInputDataCounterDom.Input.Dom).addClass(CurrentTheme.FontColor);
    AutoSchedulerDataInputDataCounterDom.Input.Dom.value = 1;
    AutoSchedulerDataInputDataCounterDom.Input.Dom.style.width = "3em";
    AutoSchedulerDataInputDataCounterDom.Input.Dom.style.height = "30px";
    AutoSchedulerDataInputDataCounterDom.Input.Dom.style.left = "10%";
    AutoSchedulerDataInputDataCounterDom.Input.Dom.style.top = 0;
    AutoSchedulerDataInputDataCounterDom.Input.Dom.style.marginTop = 0;



    EventSplits = AutoSchedulerDataInputDataCounterDom.Input;
    */






    var AutoScheduleTimeConstraintContainerID = "AutoScheduleTimeCOnstraintContainer";
    var AutoScheduleTimeConstraintContainer = getDomOrCreateNew(AutoScheduleTimeConstraintContainerID);


    var AutoScheduleContainerDataContainerID = "AutoScheduleContainerDataContainer";
    var AutoScheduleContainerDataContainer = getDomOrCreateNew(AutoScheduleContainerDataContainerID);
    var AutoScheduleContainerDataContainerSliderID = "AutoScheduleContainerDataContainerSlider";
    var AutoScheduleContainerDataContainerSlider = getDomOrCreateNew(AutoScheduleContainerDataContainerSliderID);
    AutoScheduleContainerDataContainer.Dom.appendChild(AutoScheduleContainerDataContainerSlider.Dom);
    EnableAAutoScheduleContainer.Dom.appendChild(AutoScheduleContainerDataContainer.Dom);
    EnableAAutoScheduleContainer.Dom.appendChild(EnableAutoSchedulerButton.Dom);//appending after EnableAAutoScheduleContainer because z-index effect
    var AutoScheduleDurationContainerID = "AutoScheduleDurationContainer";
    var AutoSchedulerDataInputData = { Name: "", ID: AutoScheduleDurationContainerID, Default: "Duration?" };
    var AutoSchedulerDataInputDataDom = generateuserInput(AutoSchedulerDataInputData, null);





    //var DurationDial = new Dial(0, 5, 12, 0, 0);
    var DialHolder = function () {
        this.holder = null;
    }

    DialHolder.holder = new Dial(0, 5, 12, 0, 0);
    EventNonRigidDurationHolder = DialHolder;


    var AutoScheduleDurationContainerDialContainerID = "AutoScheduleDurationContainerDialContainer";
    var AutoScheduleDurationContainerDialContainer = getDomOrCreateNew(AutoScheduleDurationContainerDialContainerID);

    removeElement(AutoSchedulerDataInputDataDom.Input.Dom);

    AutoSchedulerDataInputDataDom.Input = getDomOrCreateNew("AutoScheduleDurationContainer_Text", "span");
    AutoSchedulerDataInputDataDom.Input.Dom.innerHTML = (isTIle ? "Tile" : "Event") + " Duration";
    AutoSchedulerDataInputDataDom.FullContainer.Dom.appendChild(AutoSchedulerDataInputDataDom.Input.Dom);
    //$(AutoSchedulerDataInputDataDom.Input.Dom).click(function () { dialAutoScheduleCallBack(AutoScheduleDurationContainerDialContainer.Dom, dialAutoScheduleCallBack) });
    AutoSchedulerDataInputDataDom.FullContainer.onclick = (function () { dialAutoScheduleCallBack(AutoScheduleDurationContainerDialContainer.Dom, dialAutoScheduleCallBack) });
    $(AutoSchedulerDataInputDataDom.FullContainer.Dom).addClass(CurrentTheme.FontColor);
    //AutoSchedulerDataInputDataDom.FullContainer.Dom.style.borderBottom = "none";
    //$(AutoSchedulerDataInputDataDom.Input.Dom).addClass("InputBox");
    //AutoSchedulerDataInputDataDom.Input.Dom.style.height = "30%";
    $(AutoSchedulerDataInputDataDom.Input.Dom).addClass(CurrentTheme.FontColor);
    //AutoSchedulerDataInputDataDom.Input.Dom.style.border = "yellow 2px solid";
    //AutoSchedulerDataInputDataDom.Label.Dom.style.border = "black 2px solid";
    AutoSchedulerDataInputDataDom.Label.Dom.style.textAlign = "center";
    $(AutoSchedulerDataInputDataDom.Label.Dom).addClass("InputLabel");
    //AutoSchedulerDataInputDataDom.Input.Dom.style.width = "40%";
    AutoSchedulerDataInputDataDom.Input.Dom.style.left = "0%";
    AutoSchedulerDataInputDataDom.Input.Dom.style.top = 0;
    AutoSchedulerDataInputDataDom.Input.Dom.style.marginTop = 0;
    AutoSchedulerDataInputDataDom.Input.Dom.style.height = "30px";
    AutoSchedulerDataInputDataDom.Input.Dom.style.lineHeight = "30px";

    AutoScheduleTimeConstraintContainer.Dom.appendChild(AutoSchedulerDataInputDataDom.FullContainer.Dom);
    EnableAutoSchedulerButton.SetAsOff();//this initializs the button. This initialization has to be placed after the creation of the reference objects specified in the loop back function. In this case after the creation of "AutoScheduleContainerDataContainerSlider" in AutoScheduleContainerLoopBack






    var AutoScheduleCountContainerDialContainerID = "AutoScheduleCountContainerDialContainer";
    var AutoScheduleCountContainerDialContainer = getDomOrCreateNew(AutoScheduleCountContainerDialContainerID);





    var AutoScheduleRangeConstraintContainerID = "AutoScheduleRangeConstraintContainer";
    var AutoScheduleRangeConstraintContainer = getDomOrCreateNew(AutoScheduleRangeConstraintContainerID);
    //AutoScheduleContainerDataContainerSlider.Dom.appendChild(AutoScheduleRangeConstraintContainer.Dom);



    var AutoScheduleRangeConstraintContainerStartID = "AutoScheduleRangeConstraintContainerStart";

    var AutoScheduleRangeConstraintContainerStart = getDomOrCreateNew(AutoScheduleRangeConstraintContainerStartID);
    $(AutoScheduleRangeConstraintContainerStart.Dom).addClass("AutoScheduleTimeRange");
    var AutoScheduleRangeConstraintContainerStartDatePickerID = "AutoScheduleRangeConstraintContainerStartDatePicker";
    var AutoScheduleRangeConstraintContainerStartDatePicker = getDomOrCreateNew(AutoScheduleRangeConstraintContainerStartDatePickerID, "input");
    AutoScheduleRangeConstraintContainerStartDatePicker.Dom.setAttribute('readonly', 'readonly');
    AutoScheduleRangeConstraintContainerStartDatePicker.Dom.setAttribute('disabled', 'true');
    setTimeout(function () {
        AutoScheduleRangeConstraintContainerStartDatePicker.Dom.blur();  //actually close the keyboard
        // Remove readonly attribute after keyboard is hidden.
        AutoScheduleRangeConstraintContainerStartDatePicker.Dom.removeAttribute('readonly');
        AutoScheduleRangeConstraintContainerStartDatePicker.Dom.removeAttribute('disabled');
    }, 100);

    BindImputToDatePicketMobile(AutoScheduleRangeConstraintContainerStartDatePicker);

    /*
    $(AutoScheduleRangeConstraintContainerStartDatePicker.Dom).click(
        function ()
        {
            var Container = getDomOrCreateNew("ContainerDateElement");
            LaunchDatePicker(false, Container.Dom, AutoScheduleRangeConstraintContainerStartDatePicker.Dom);
            Container.Dom.style.display = "block";
            CurrentTheme.getCurrentContainer().appendChild((Container.Dom));
        }
    );
    */

    var AutoScheduleRangeConstraintContainerStartTimePickerID = "AutoScheduleRangeConstraintContainerStartTimePicker";
    var AutoScheduleRangeConstraintContainerStartTimePicker = getDomOrCreateNew(AutoScheduleRangeConstraintContainerStartTimePickerID, "input");


    AutoScheduleRangeConstraintContainerStart.Dom.appendChild(AutoScheduleRangeConstraintContainerStartTimePicker.Dom)
    AutoScheduleRangeConstraintContainerStart.Dom.appendChild(AutoScheduleRangeConstraintContainerStartDatePicker.Dom)//placing after time picker because of tab input
    $(AutoScheduleRangeConstraintContainerStartDatePicker.Dom).addClass("DatePicker");
    $(AutoScheduleRangeConstraintContainerStartTimePicker.Dom).addClass("TimePicker");

    AutoScheduleRangeConstraintContainerStartTimePicker.Dom.style.borderBottom = "6px solid";
    $(AutoScheduleRangeConstraintContainerStartTimePicker.Dom).addClass(CurrentTheme.BorderColor);


    //$(AutoScheduleRangeConstraintContainerStartTimePicker.Dom).timepicker();

    AutoScheduleRangeConstraintContainerStartTimePicker.Dom.value = "";
    AutoScheduleRangeConstraintContainerStartTimePicker.Dom.setAttribute("placeholder", "Start Time (Default:Now) hh:mm am/pm");
    $(AutoScheduleRangeConstraintContainerStartTimePicker.Dom).addClass(CurrentTheme.FontColor);
    AutoScheduleRangeConstraintContainerStartDatePicker.Dom.value = "";
    AutoScheduleRangeConstraintContainerStartDatePicker.Dom.setAttribute("placeholder", "Start Date (Default:Today) mm/dd/yyyy");
    $(AutoScheduleRangeConstraintContainerStartDatePicker.Dom).addClass(CurrentTheme.FontColor);

    //$(AutoScheduleRangeConstraintContainerStartDatePicker.Dom).datepicker();


    var AutoScheduleRangeConstraintContainerStartLabelID = "AutoScheduleRangeConstraintContainerStartLabel";
    var AutoScheduleRangeConstraintContainerStartLabel = getDomOrCreateNew(AutoScheduleRangeConstraintContainerStartLabelID);
    AutoScheduleRangeConstraintContainerStartLabel.Dom.innerHTML = "Start";
    $(AutoScheduleRangeConstraintContainerStartLabel.Dom).addClass("RangeLabel");
    $(AutoScheduleRangeConstraintContainerStartLabel.Dom).addClass(CurrentTheme.FontColor);
    //AutoScheduleRangeConstraintContainerStart.Dom.appendChild(AutoScheduleRangeConstraintContainerStartLabel.Dom);




    var AutoScheduleRangeConstraintContainerEndID = "AutoScheduleRangeConstraintContainerEnd";
    var AutoScheduleRangeConstraintContainerEnd = getDomOrCreateNew(AutoScheduleRangeConstraintContainerEndID);
    $(AutoScheduleRangeConstraintContainerEnd.Dom).addClass("AutoScheduleTimeRange");
    var AutoScheduleRangeConstraintContainerEndDatePickerID = "AutoScheduleRangeConstraintContainerEndDatePicker";
    var AutoScheduleRangeConstraintContainerEndDatePicker = getDomOrCreateNew(AutoScheduleRangeConstraintContainerEndDatePickerID, "input");
    AutoScheduleRangeConstraintContainerEndDatePicker.Dom.setAttribute('readonly', 'readonly');
    AutoScheduleRangeConstraintContainerEndDatePicker.Dom.setAttribute('disabled', 'true');
    setTimeout(function () {
        AutoScheduleRangeConstraintContainerEndDatePicker.Dom.blur();  //actually close the keyboard
        // Remove readonly attribute after keyboard is hidden.
        AutoScheduleRangeConstraintContainerEndDatePicker.Dom.removeAttribute('readonly');
        AutoScheduleRangeConstraintContainerEndDatePicker.Dom.removeAttribute('disabled');
    }, 100);

    $(AutoScheduleRangeConstraintContainerEndDatePicker.Dom).click(
        function () {
            var Container = getDomOrCreateNew("ContainerDateElement");
            LaunchDatePicker(false, Container.Dom, AutoScheduleRangeConstraintContainerEndDatePicker.Dom);
            Container.Dom.style.display = "block";
            CurrentTheme.getCurrentContainer().appendChild((Container.Dom));
        }
    );



    var AutoScheduleRangeConstraintContainerEndTimePickerID = "AutoScheduleRangeConstraintContainerEndTimePicker";
    var AutoScheduleRangeConstraintContainerEndTimePicker = getDomOrCreateNew(AutoScheduleRangeConstraintContainerEndTimePickerID, "input");


    AutoScheduleRangeConstraintContainerEnd.Dom.appendChild(AutoScheduleRangeConstraintContainerEndTimePicker.Dom);
    AutoScheduleRangeConstraintContainerEnd.Dom.appendChild(AutoScheduleRangeConstraintContainerEndDatePicker.Dom);//placing after time picker because of tab button press. I want site to go from Time picker to Date
    $(AutoScheduleRangeConstraintContainerEndDatePicker.Dom).addClass("DatePicker");
    $(AutoScheduleRangeConstraintContainerEndTimePicker.Dom).addClass("TimePicker");
    //$(AutoScheduleRangeConstraintContainerEndTimePicker.Dom).timepicker();


    AutoScheduleRangeConstraintContainerEndTimePicker.Dom.value = "11:59PM";
    AutoScheduleRangeConstraintContainerEndTimePicker.Dom.style.borderBottom = "6px solid";
    $(AutoScheduleRangeConstraintContainerEndTimePicker.Dom).addClass(CurrentTheme.BorderColor);

    $(AutoScheduleRangeConstraintContainerEndTimePicker.Dom).addClass(CurrentTheme.FontColor);
    AutoScheduleRangeConstraintContainerEndDatePicker.Dom.setAttribute("placeholder", "End Date (Default: Tomorrow) mm/dd/yyyy");
    $(AutoScheduleRangeConstraintContainerEndDatePicker.Dom).addClass(CurrentTheme.FontColor);

    //$(AutoScheduleRangeConstraintContainerEndDatePicker.Dom).datepicker();


    AutoScheduleRangeConstraintContainerStart.getDateTimeData = getFullTimeFromEntry(AutoScheduleRangeConstraintContainerStartTimePicker, AutoScheduleRangeConstraintContainerStartDatePicker, 0)
    EventStartDateTimeDom = AutoScheduleRangeConstraintContainerStart;

    AutoScheduleRangeConstraintContainerEnd.getDateTimeData = getFullTimeFromEntry(AutoScheduleRangeConstraintContainerEndTimePicker, AutoScheduleRangeConstraintContainerEndDatePicker, OneDayInMs);
    EventEndDateTimeDom = AutoScheduleRangeConstraintContainerEnd;

    var AutoScheduleRangeConstraintContainerEndLabelID = "AutoScheduleRangeConstraintContainerEndLabel";
    var AutoScheduleRangeConstraintContainerEndLabel = getDomOrCreateNew(AutoScheduleRangeConstraintContainerEndLabelID);
    AutoScheduleRangeConstraintContainerEndLabel.Dom.innerHTML = "End";
    $(AutoScheduleRangeConstraintContainerEndLabel.Dom).addClass("RangeLabel");
    $(AutoScheduleRangeConstraintContainerEndLabel.Dom).addClass(CurrentTheme.FontColor);
    //AutoScheduleRangeConstraintContainerEnd.Dom.appendChild(AutoScheduleRangeConstraintContainerEndLabel.Dom);

    var CalEventIdentifierSectionCompletionID = "CalEventIdentifierSectionCompletion";
    var CalEventIdentifierSectionCompletion = getDomOrCreateNew(CalEventIdentifierSectionCompletionID);
    $(CalEventIdentifierSectionCompletion.Dom).addClass("SectionCompletionButton");
    $(CalEventIdentifierSectionCompletion.Dom).addClass(CurrentTheme.AlternateContentSection);
    $(CalEventIdentifierSectionCompletion.Dom).addClass(CurrentTheme.AlternateFontColor);









    AutoScheduleRangeConstraintContainer.Dom.appendChild(AutoScheduleRangeConstraintContainerStart.Dom);
    AutoScheduleRangeConstraintContainer.Dom.appendChild(AutoScheduleTimeConstraintContainer.Dom);//readjust
    AutoScheduleContainerDataContainerSlider.Dom.appendChild(AutoScheduleRangeConstraintContainerEnd.Dom);//readjust again
    //AutoScheduleContainerDataContainerSlider.Dom.appendChild(AutoSchedulerDataInputDataCounterDom.FullContainer.Dom);//append number of split input box





    /* Auto Schedule Container End */


    function getCurrentRangeOfAutoContainer() {

        if (!EnableAutoSchedulerButton.status) {
            return 0;
        }
        var StartData = getFullTimeFromEntry(AutoScheduleRangeConstraintContainerStartTimePicker, AutoScheduleRangeConstraintContainerStartDatePicker, 0)();
        var EndData = getFullTimeFromEntry(AutoScheduleRangeConstraintContainerEndTimePicker, AutoScheduleRangeConstraintContainerEndDatePicker, OneDayInMs)();

        var TwentyFourHourStartTime = StartData.Time //AP_To24Hour(AutoScheduleRangeConstraintContainerStartTimePicker.Dom.value);
        var TwentyFourHourEndTime = EndData.Time;//AP_To24Hour(AutoScheduleRangeConstraintContainerEndTimePicker.Dom.value);
        var StartDate = StartData.Date// date_mm_dd__yyyy_ToDateObj(AutoScheduleRangeConstraintContainerStartDatePicker.Dom.value,"/")
        var EndDate = EndData.Date//date_mm_dd__yyyy_ToDateObj(AutoScheduleRangeConstraintContainerEndDatePicker.Dom.value, "/")
        StartDate.setHours(TwentyFourHourStartTime.Hour, TwentyFourHourStartTime.Minute);
        EndDate.setHours(TwentyFourHourEndTime.Hour, TwentyFourHourEndTime.Minute);


        return EndDate - StartDate;
    }






    function getFullTimeFromEntrytoJSDateobj(Obj) {
        var Time = Tesobj.Time;

        var DateData = new Date(Tesobj.Date);
        DateData.setHours(Time.Hour, Time.Minute);
        return DateData;
    }


    function dialAutoScheduleCallBack(value) {
        DialOnEvent(null, UpdateInputBox, DialHolder.holder);
    }


    function UpdateInputBox(myDial) {
        DialHolder.holder = myDial;
        AutoSchedulerDataInputDataDom.Input.Dom.innerHTML = DialHolder.holder.getTimeString();
    }

    function AutoScheduleContainerLoopBack() {
        if (!EnableAutoSchedulerButton.status) {

            $(EnableAAutoScheduleContainer.Dom).addClass("setAsDisplayNone");//sets as hidden
            setTimeout(function () {//later relases the slider
                $(AutoScheduleContainerDataContainerSlider.Dom).removeClass("EnableSliderTop");
                $(AutoScheduleContainerDataContainerSlider.Dom).addClass("DisableSliderTop");
            }, 100);

            //$(AutoSchedulerDataInputDataCounterDom.FullContainer.Dom).hide();

            if (AutoScheduleRangeConstraintContainer != null) {
                //AutoScheduleRangeConstraintContainer.Dom.style.top = "0%";//hack Alert
            }
        }
        else {

            $(EnableAAutoScheduleContainer.Dom).removeClass("setAsDisplayNone");
            setTimeout(function () {
                $(AutoScheduleContainerDataContainerSlider.Dom).removeClass("DisableSliderTop");
                $(AutoScheduleContainerDataContainerSlider.Dom).addClass("EnableSliderTop");
            }, 100);

            //$(AutoSchedulerDataInputDataCounterDom.FullContainer.Dom).show();
            if (AutoScheduleRangeConstraintContainer != null) {
                //AutoScheduleRangeConstraintContainer.Dom.style.top = "50%";//hack Alert
            }

        }
    }

    var ColorPickerButton = GenerateColorPickerContainer(CurrentTheme.getCurrentContainer());
    EventColor = ColorPickerButton.Picker.Selector;
    NameTabContent.Dom.appendChild(ColorPickerButton.Button);

    NameTabContent.Dom.appendChild(CalEventNameInput.Dom);
    NameTabContent.Dom.appendChild(CalEventAddressInput.Dom);
    NameTabContent.Dom.appendChild(CalEventAddressTagInput.Dom);
    NameTabContent.Dom.appendChild(AutoScheduleRangeConstraintContainer.Dom);

    NameTabContent.Dom.appendChild(EnableAAutoScheduleContainer.Dom);



    NameTabContent.Dom.appendChild(CalEventIdentifierSectionCompletion.Dom);//disables recurrence tab selection



    if (isTIle) {
        EnableAutoSchedulerButton.SetAsOn();
    }
    else {
        EnableAutoSchedulerButton.SetAsOff();
    }

    function LaunchTab() {

    }

    function DisableTab() {

    }

    NameTabContent.getCurrentRangeOfAutoContainer = getCurrentRangeOfAutoContainer;//appends function to content in order to get Current Range, this can be called by any other tab;
    var retValue = { Name: "New Event Tab", Title: NameTabTitle, Content: NameTabContent, Completion: CalEventIdentifierSectionCompletion, LaunchTab: LaunchTab, DisableTab: DisableTab };


    $(AutoScheduleRangeConstraintContainerEndTimePicker.Dom).datebox({
        mode: "timeflipbox",
        minuteStep: 5,
        overrideTimeFormat: 12,
        overrideTimeOutput: "%I:%M%p"
    });

    AutoScheduleRangeConstraintContainerEndTimePicker.onclick = function () {
        var aElement1 = $(AutoScheduleRangeConstraintContainerEndTimePicker.parentElement).children("a");
        aElement1[0].click()
    };

    $(AutoScheduleRangeConstraintContainerStartTimePicker.Dom).datebox({
        mode: "timeflipbox",
        minuteStep: 5,
        overrideTimeFormat: 12,
        overrideTimeOutput: "%I:%M%p"
    });


    AutoScheduleRangeConstraintContainerStartTimePicker.onclick = function () {
        var aElement2 = $(AutoScheduleRangeConstraintContainerStartTimePicker.parentElement).children("a");
        aElement2[0].click()
    };
    return retValue;
}

function createCalEventRecurrenceTab(IsTile) {
    var RecurrenceTabContainerID = "RecurrenceTabContainer";
    var RecurrenceTabContainer = getDomOrCreateNew(RecurrenceTabContainerID);
    var RecurrenceTabTitleID = "RecurrenceTabTitle";
    var RecurrenceTabTitle = getDomOrCreateNew(RecurrenceTabTitleID);
    var RecurrenceTabTitleTextID = "RecurrenceTabTitleText";
    var HorizontalLine = InsertHorizontalLine("100%", "00%", "100%", "4px");
    HorizontalLine.style.marginTop = "-4px";
    RecurrenceTabTitle.Dom.appendChild(HorizontalLine);

    var RecurrenceTabTitleText = getDomOrCreateNew(RecurrenceTabTitleTextID);
    RecurrenceTabTitleText.Dom.innerHTML = "Recurrence";
    var RecurrenceTabContentID = "RecurrenceTabContent";
    var RecurrenceTabContent = getDomOrCreateNew(RecurrenceTabContentID);
    RecurrenceTabContent.Misc = { Selection: null };
    RecurrenceTabTitle.Dom.appendChild(RecurrenceTabTitleText.Dom);
    $(RecurrenceTabTitleText.Dom).addClass("TabTitleText");
    $(RecurrenceTabTitle.Dom).addClass("TabTitle");
    $(RecurrenceTabContent.Dom).addClass("TabContent");

    //Enable Recurrence
    var EnableRecurrenceContainerID = "EnableRecurrenceContainer";
    var EnableRecurrenceContainer = getDomOrCreateNew(EnableRecurrenceContainerID);

    var EnableRecurrenceLabelID = "EnableRecurrenceLabel";
    var EnableRecurrenceLabel = getDomOrCreateNew(EnableRecurrenceLabelID, "label");
    EnableRecurrenceContainer.Dom.appendChild(EnableRecurrenceLabel.Dom);
    $(EnableRecurrenceContainer.Dom).addClass(CurrentTheme.FontColor);
    EnableRecurrenceLabel.Dom.innerHTML = "Do you want this event to recurr?"

    var EnableRecurrenceButtonContainerID = "EnableRecurrenceButtonContainer";
    var EnableRecurrenceButtonContainer = getDomOrCreateNew(EnableRecurrenceButtonContainerID);
    EnableRecurrenceContainer.Dom.appendChild(EnableRecurrenceButtonContainer.Dom);
    $(EnableRecurrenceButtonContainer.Dom).addClass("EnableButtonContainer");

    var EnableRecurrenceButtonID = "EnableRecurrenceButton";
    var EnableRecurrenceButton = getDomOrCreateNew(EnableRecurrenceButtonID);
    $(EnableRecurrenceButton.Dom).addClass("EnableButton");

    EnableRecurrenceButton.status = 0;
    EventrepeatStatus = EnableRecurrenceButton;


    //Enabled Recurring Settings
    var EnabledRecurrenceContainerID = "EnabledRecurrenceContainer";
    var EnabledRecurrenceContainer = getDomOrCreateNew(EnabledRecurrenceContainerID);


    var EnableRecurrenceYesTextID = "EnableRecurrenceYesText";
    var EnableRecurrenceYesText = getDomOrCreateNew(EnableRecurrenceYesTextID);
    EnableRecurrenceButtonContainer.Dom.appendChild(EnableRecurrenceYesText.Dom);
    $(EnableRecurrenceYesText.Dom).addClass("EnableButtonChoiceText");
    $(EnableRecurrenceYesText.Dom).addClass("EnableButtonChoiceYeaText");


    var EnableRecurrenceNoTextID = "EnableRecurrenceNoText";
    var EnableRecurrenceNoText = getDomOrCreateNew(EnableRecurrenceNoTextID);
    EnableRecurrenceButtonContainer.Dom.appendChild(EnableRecurrenceNoText.Dom);
    EnableRecurrenceButtonContainer.Dom.appendChild(EnableRecurrenceButton.Dom);//appending after because of z-index effect
    $(EnableRecurrenceNoText.Dom).addClass("EnableButtonChoiceText");
    $(EnableRecurrenceNoText.Dom).addClass("EnableButtonChoiceNayText");

    RecurrenceTabContent.Dom.appendChild(EnableRecurrenceContainer.Dom);
    $(EnableRecurrenceContainer.Dom).click(genFunctionForButtonClick(EnableRecurrenceButton, EnabledRecurrenceContainer));
    genFunctionForButtonClick(EnableRecurrenceButton, EnabledRecurrenceContainer)();
    EventrepeatStatus = EnabledRecurrenceContainer;



    function genFunctionForButtonClick(Button, RecurrenceContainer) {
        return function () {
            switch (Button.status) {
                case 0:
                    {
                        $(Button.Dom).removeClass("EnableButtonOn");
                        $(Button.Dom).addClass("EnableButtonOff");
                        $(RecurrenceContainer.Dom).hide();
                        RecurrenceContainer.status = 0;
                    }
                    break;
                case 1:
                    {


                        $(Button.Dom).removeClass("EnableButtonOff");
                        $(Button.Dom).addClass("EnableButtonOn");
                        $(RecurrenceContainer.Dom).show();
                        RecurrenceContainer.status = 1;
                    }
                    break;
            }
            Button.status += 1;
            Button.status %= 2;
        }
    }






    /*Recurrence Button Start*/
    var RecurrenceButtonContainerID = "RecurrenceButtonContainer"
    var RecurrenceButtonContainer = getDomOrCreateNew(RecurrenceButtonContainerID);

    var dailyRecurrenceButtonID = "dailyRecurrenceButton";
    var dailyRecurrenceButton = getDomOrCreateNew(dailyRecurrenceButtonID);
    dailyRecurrenceButton.Range = OneDayInMs;
    dailyRecurrenceButton.Type = { Name: "Daily", Index: 0 };
    dailyRecurrenceButton.Misc = null;
    var dailyRecurrenceButtonTextID = "dailyRecurrenceButtonText";
    var dailyRecurrenceButtonText = getDomOrCreateNew(dailyRecurrenceButtonTextID);
    dailyRecurrenceButtonText.Dom.innerHTML = "Daily"
    $(dailyRecurrenceButton.Dom).addClass("recurrenceButton");
    $(dailyRecurrenceButtonText.Dom).addClass("CentreAlignedName");
    $(dailyRecurrenceButton.Dom).append(dailyRecurrenceButtonText.Dom);


    var weeklyRecurrenceButtonID = "weeklyRecurrenceButton";
    var weeklyRecurrenceButton = getDomOrCreateNew(weeklyRecurrenceButtonID);
    weeklyRecurrenceButton.Range = OneWeekInMs;
    weeklyRecurrenceButton.Type = { Name: "Weekly", Index: 1 };
    weeklyRecurrenceButton.Misc = null;
    var weeklyRecurrenceButtonTextID = "weeklyRecurrenceButtonText";
    var weeklyRecurrenceButtonText = getDomOrCreateNew(weeklyRecurrenceButtonTextID);
    $(weeklyRecurrenceButton.Dom).addClass("recurrenceButton");
    $(weeklyRecurrenceButtonText.Dom).addClass("CentreAlignedName");
    weeklyRecurrenceButtonText.Dom.innerHTML = "Weekly"
    $(weeklyRecurrenceButton.Dom).append(weeklyRecurrenceButtonText.Dom);


    var monthlyRecurrenceButtonID = "monthlyRecurrenceButton";
    var monthlyRecurrenceButton = getDomOrCreateNew(monthlyRecurrenceButtonID);
    monthlyRecurrenceButton.Range = FourWeeksInMs;
    monthlyRecurrenceButton.Type = { Name: "Monthly", Index: 2 };
    var monthlyRecurrenceButtonTextID = "monthlyRecurrenceButtonText";
    var monthlyRecurrenceButtonText = getDomOrCreateNew(monthlyRecurrenceButtonTextID);
    $(monthlyRecurrenceButton.Dom).addClass("recurrenceButton");
    monthlyRecurrenceButtonText.Dom.innerHTML = "Monthly"
    $(monthlyRecurrenceButtonText.Dom).addClass("CentreAlignedName");
    $(monthlyRecurrenceButton.Dom).append(monthlyRecurrenceButtonText.Dom);


    var yearlyRecurrenceButtonID = "yearlyRecurrenceButton";
    var yearlyRecurrenceButton = getDomOrCreateNew(yearlyRecurrenceButtonID);
    yearlyRecurrenceButton.Range = OneYearInMs;
    yearlyRecurrenceButton.Type = { Name: "Yearly", Index: 3 };
    yearlyRecurrenceButton.Misc = null;
    var yearlyRecurrenceButtonTextID = "yearlyRecurrenceButtonText";
    var yearlyRecurrenceButtonText = getDomOrCreateNew(yearlyRecurrenceButtonTextID);
    $(yearlyRecurrenceButton.Dom).addClass("recurrenceButton");
    yearlyRecurrenceButtonText.Dom.innerHTML = "Yearly"
    $(yearlyRecurrenceButtonText.Dom).addClass("CentreAlignedName");
    $(yearlyRecurrenceButton.Dom).append(yearlyRecurrenceButtonText.Dom);


    $(dailyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateContentSection);
    $(weeklyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateContentSection);
    $(monthlyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateContentSection);
    $(yearlyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateContentSection);

    $(dailyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateFontColor);
    $(weeklyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateFontColor);
    $(monthlyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateFontColor);
    $(yearlyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateFontColor);

    var DaysOfTheWeekContainerID = "DaysOfTheWeekContainer";
    var DaysOfTheWeekContainer = getDomOrCreateNew(DaysOfTheWeekContainerID);
    $(DaysOfTheWeekContainer.Dom).addClass("DaysOfTheWeekContainer");
    var DaysOfTheWeek = generateDayOfWeekRepetitionDom();
    weeklyRecurrenceButton.Misc = DaysOfTheWeek
    DaysOfTheWeek.AllDoms.forEach(function (eachDom) { DaysOfTheWeekContainer.Dom.appendChild(eachDom.Dom) });
    $(DaysOfTheWeekContainer.Dom).addClass(CurrentTheme.FontColor);


    var AllDoms = [dailyRecurrenceButton, weeklyRecurrenceButton, monthlyRecurrenceButton, yearlyRecurrenceButton];//order of AllDOms matters, smallest range to largest to maintain correctness of application

    EventRepetitionSelection = AllDoms;//globals Access


    function DisableAllDomData() {
        AllDoms.forEach(function (eachDomCombo) { $(eachDomCombo.Dom).addClass("InActiveRecurrenceButton"); eachDomCombo.status = false });
    }




    function createDomEnablingFunction(DomCombo, Index, RecurrenceTabContent, DayOfTheWeekContainer, RevealDayOfWeekCircles, EventRange) {
        var var1 = DaysOfTheWeekContainer;
        var CallBack = function () {
            DisableAllDomData();
            $(DomCombo.Dom).removeClass("InActiveRecurrenceButton");
            $(DomCombo.Dom).addClass("ActiveRecurrenceButton");
            DomCombo.status = true;
            RecurrenceTabContent.Misc.Selection = Index;
            var var2 = var1;
            if (Index != 1)//checks if weekly is selected 
            {
                $(var2.Dom).hide();
            }
            else {
                var2.Dom.style.visible = "visible";
                $(var2.Dom).show();
                RevealDayOfWeekCircles();
            }
        }

        return CallBack;
    }


    $(dailyRecurrenceButton.Dom).click(createDomEnablingFunction(AllDoms[0], 0, RecurrenceTabContent, DaysOfTheWeekContainer, DaysOfTheWeek.RevealDayOfWeek));
    $(weeklyRecurrenceButton.Dom).click(createDomEnablingFunction(AllDoms[1], 1, RecurrenceTabContent, DaysOfTheWeekContainer, DaysOfTheWeek.RevealDayOfWeek));
    $(monthlyRecurrenceButton.Dom).click(createDomEnablingFunction(AllDoms[2], 2, RecurrenceTabContent, DaysOfTheWeekContainer, DaysOfTheWeek.RevealDayOfWeek));
    $(yearlyRecurrenceButton.Dom).click(createDomEnablingFunction(AllDoms[3], 3, RecurrenceTabContent, DaysOfTheWeekContainer, DaysOfTheWeek.RevealDayOfWeek));






    /*RecurrenceButtonContainer.Dom.appendChild(dailyRecurrenceButton.Dom);
    RecurrenceButtonContainer.Dom.appendChild(weeklyRecurrenceButton.Dom);
    RecurrenceButtonContainer.Dom.appendChild(monthlyRecurrenceButton.Dom);
    RecurrenceButtonContainer.Dom.appendChild(yearlyRecurrenceButton.Dom);*/
    /*Recurrence Button End*/



    /*Recurrence Day Preference Container Start*/
    var DayPreferenceContainerID = "DayPreferenceContainer";
    var DayPreferenceContainer = getDomOrCreateNew(DayPreferenceContainerID);

    var BussinessHourContainerID = "BussinessHourInputContainer";
    var BussinessHourInputContainer = getDomOrCreateNew(BussinessHourContainerID);
    $(BussinessHourInputContainer.Dom).addClass(CurrentTheme.FontColor);
    $(BussinessHourInputContainer.Dom).addClass("DayPreferenceRadioButtonContainer")

    var BussinessHourRadioID = "BussinessHourRadio";
    var BussinessHourRadio = getDomOrCreateNew(BussinessHourRadioID, "input");
    BussinessHourRadio.Dom.setAttribute("type", "radio");
    BussinessHourRadio.Dom.setAttribute("value", "0");
    BussinessHourRadio.Dom.setAttribute("name", "DayPreference");
    var BussinessHourRadioLabelID = "BussinessHourRadioLabel"
    var BussinessHourRadioLabel = getDomOrCreateNew(BussinessHourRadioLabelID, "label");
    BussinessHourRadioLabel.Dom.setAttribute("for", "BussinessHourRadio");
    BussinessHourRadioLabel.Dom.innerHTML = "Work Days";


    BussinessHourInputContainer.Dom.appendChild(BussinessHourRadio.Dom)
    BussinessHourInputContainer.Dom.appendChild(BussinessHourRadioLabel.Dom)


    var AnyHourTimeContainerID = "AnyHourTimeContainer";
    var AnyHourTimeContainer = getDomOrCreateNew(AnyHourTimeContainerID);
    $(AnyHourTimeContainer.Dom).addClass(CurrentTheme.FontColor);
    $(AnyHourTimeContainer.Dom).addClass("DayPreferenceRadioButtonContainer")
    var AnytimeID = "AnyHourTimeRadio";
    var Anytime = getDomOrCreateNew(AnytimeID, "input");
    Anytime.Dom.setAttribute("type", "radio");
    Anytime.Dom.setAttribute("value", "1");
    Anytime.Dom.setAttribute("name", "DayPreference");
    var AnytimeLabelID = "AnytimeLabel"
    var AnytimeLabel = getDomOrCreateNew(AnytimeLabelID, "label");
    AnytimeLabel.Dom.setAttribute("for", "AnyHourTimeRadio");
    AnytimeLabel.Dom.innerHTML = "AnyTime";

    Anytime.checked = true;



    AnyHourTimeContainer.Dom.appendChild(Anytime.Dom)
    AnyHourTimeContainer.Dom.appendChild(AnytimeLabel.Dom)

    var CustomHourTimeContainerID = "CustomHourTimeContainer";
    var CustomHourTimeContainer = getDomOrCreateNew(CustomHourTimeContainerID);
    $(CustomHourTimeContainer.Dom).addClass(CurrentTheme.FontColor);
    $(CustomHourTimeContainer.Dom).addClass("DayPreferenceRadioButtonContainer")
    var CustomtimeID = "CustomHourTimeRadio";
    var Customtime = getDomOrCreateNew(CustomtimeID, "input");
    Customtime.Dom.setAttribute("type", "radio");
    Customtime.Dom.setAttribute("value", "1");
    Customtime.Dom.setAttribute("name", "DayPreference");
    var CustomtimeLabelID = "CustomtimeLabel"
    var CustomtimeLabel = getDomOrCreateNew(CustomtimeLabelID, "label");
    CustomtimeLabel.Dom.setAttribute("for", "CustomHourTimeRadio");
    CustomtimeLabel.Dom.innerHTML = "CustomTime";
    $(CustomtimeLabel.Dom).addClass("DayPreferenceLabel");

    CustomHourTimeContainer.Dom.appendChild(Customtime.Dom)
    CustomHourTimeContainer.Dom.appendChild(CustomtimeLabel.Dom)

    var TimeSelection = getDomOrCreateNew("CustomTimeBar");
    var StartTimeContainer = getDomOrCreateNew("StartTimeInputContainer");
    var StartTime = getDomOrCreateNew("StartTimeInput", "input");
    StartTimeContainer.appendChild(StartTime);
    var ToText = getDomOrCreateNew("ToText", "span");
    ToText.innerHTML = " to "
    var EndTimeContainer = getDomOrCreateNew("EndTimeInputContainer");
    var EndTime = getDomOrCreateNew("EndTimeInput", "input");
    EndTimeContainer.appendChild(EndTime);


    TimeSelection.appendChild(StartTimeContainer);
    TimeSelection.appendChild(ToText);
    TimeSelection.appendChild(EndTimeContainer);


    DayPreferenceContainer.Dom.appendChild(AnyHourTimeContainer.Dom)
    DayPreferenceContainer.Dom.appendChild(BussinessHourInputContainer.Dom)
    DayPreferenceContainer.Dom.appendChild(CustomHourTimeContainer.Dom)
    DayPreferenceContainer.Dom.appendChild(TimeSelection.Dom);

    $(StartTime.Dom).datebox({
        mode: "timeflipbox",
        minuteStep: 5,
        overrideTimeFormat: 12,
        overrideTimeOutput: "%I:%M%p"
    });

    $(EndTime.Dom).datebox({
        mode: "timeflipbox",
        minuteStep: 5,
        overrideTimeFormat: 12,
        overrideTimeOutput: "%I:%M%p"
    });
    EventRestrictionFalseFlag = Anytime;
    EventRestrictionStart = StartTime;
    EventRestrictionEnd = EndTime;
    EventRestrictionIsWorkWeek = BussinessHourRadio;

    function BindChangesToRadioButton() {
        Anytime.onchange = function () {
            if (Anytime.checked) {
                $(TimeSelection).addClass("setAsDisplayNone");
            }
        }

        BussinessHourRadio.onchange = function () {
            if (BussinessHourRadio.checked) {
                $(TimeSelection).removeClass("setAsDisplayNone");
                if (StartTime.value == "") {
                    StartTime.value = "9:00 am";
                }

                if (EndTime.value == "") {
                    EndTime.value = "6:00 pm";
                }
            }
        }

        Customtime.onchange = function () {
            if (Customtime.checked) {
                $(TimeSelection).removeClass("setAsDisplayNone");
                if (StartTime.value == "") {
                    StartTime.value = "12:00 am";
                }

                if (EndTime.value == "") {
                    EndTime.value = "11:59 pm";
                }
            }
        }


        Anytime.onchange();
        BussinessHourRadio.onchange();
        Customtime.onchange();

        var aElement1 = $(StartTimeContainer).children("a");
        StartTimeContainer.onclick = function () {
            aElement1[0].click()
        };

        var aElement2 = $(EndTimeContainer).children("a");
        EndTimeContainer.onclick = function () {
            aElement2[0].click()
        };
    }


    BindChangesToRadioButton();
    /*Repeat event container Start*/
    var RepetitionRangeCOntainerID = "RepetitionRangeContainer";
    var RepetitionRangeCOntainer = getDomOrCreateNew(RepetitionRangeCOntainerID);
    $(RepetitionRangeCOntainer.Dom).addClass(CurrentTheme.FontColor);
    var RepetitionRangeContainerStartContainerID = "RepetitionRangeContainerStartContainer";
    var RepetitionRangeContainerStartContainer = getDomOrCreateNew(RepetitionRangeContainerStartContainerID);
    var RepetitionRangeContainerStartContainerLabelID = "RepetitionRangeContainerStartContainerLabel";
    var RepetitionRangeContainerStartContainerLabel = getDomOrCreateNew(RepetitionRangeContainerStartContainerLabelID);
    RepetitionRangeContainerStartContainerLabel.Dom.innerHTML = "Repeat Start:";
    $(RepetitionRangeContainerStartContainerLabel.Dom).addClass("DateInputLabel");
    var RepetitionRangeContainerStartInputID = "RepetitionRangeContainerStartInput";
    var RepetitionRangeContainerStartInput = getDomOrCreateNew(RepetitionRangeContainerStartInputID, "input");
    $(RepetitionRangeContainerStartContainer.Dom).append(RepetitionRangeContainerStartContainerLabel.Dom);
    $(RepetitionRangeContainerStartContainer.Dom).append(RepetitionRangeContainerStartInput.Dom);
    $(RepetitionRangeContainerStartInput.Dom).addClass("DateInputData");
    //$(RepetitionRangeContainerStartInput.Dom).datepicker();
    EventRepeatStart = RepetitionRangeContainerStartInput;

    RepetitionRangeCOntainer.Dom.appendChild(RepetitionRangeContainerStartContainer.Dom);


    var RepetitionRangeContainerEndContainerID = "RepetitionRangeContainerEndContainer";
    var RepetitionRangeContainerEndContainer = getDomOrCreateNew(RepetitionRangeContainerEndContainerID);
    var RepetitionRangeContainerEndContainerLabelID = "RepetitionRangeContainerEndContainerLabel";
    var RepetitionRangeContainerEndContainerLabel = getDomOrCreateNew(RepetitionRangeContainerEndContainerLabelID);
    RepetitionRangeContainerEndContainerLabel.Dom.innerHTML = "Repeat End:";
    $(RepetitionRangeContainerEndContainerLabel.Dom).addClass("DateInputLabel");

    var RepetitionRangeContainerEndInputID = "RepetitionRangeContainerEndInput";
    var RepetitionRangeContainerEndInput = getDomOrCreateNew(RepetitionRangeContainerEndInputID, "input");


    $(RepetitionRangeContainerEndContainer.Dom).append(RepetitionRangeContainerEndContainerLabel.Dom);
    $(RepetitionRangeContainerEndContainer.Dom).append(RepetitionRangeContainerEndInput.Dom);
    $(RepetitionRangeContainerEndInput.Dom).addClass("DateInputData");
    BindImputToDatePicketMobile(RepetitionRangeContainerEndInput);
    //$(RepetitionRangeContainerEndInput.Dom).datepicker();
    EventRepeatEnd = RepetitionRangeContainerEndInput;

    RepetitionRangeCOntainer.Dom.appendChild(RepetitionRangeContainerEndContainer.Dom);

    /*Repeat event container End*/

    var RecurrenceSectionCompletionID = "RecurrenceSectionCompletion";
    var RecurrenceSectionCompletion = getDomOrCreateNew(RecurrenceSectionCompletionID);
    $(RecurrenceSectionCompletion.Dom).addClass("SectionCompletionButton");
    $(RecurrenceSectionCompletion.Dom).addClass(CurrentTheme.AlternateContentSection);


    /*Split count input box*/
    var AutoScheduleCountContainerID = "AutoScheduleCountContainer";
    var AutoSchedulerDataInputData = { Name: "Number Of Splits", ID: AutoScheduleCountContainerID, Default: "Splits?" };
    var AutoSchedulerDataInputDataCounterDom = generateuserInput(AutoSchedulerDataInputData, null);
    $(AutoSchedulerDataInputDataCounterDom.FullContainer.Dom).addClass(CurrentTheme.FontColor);
    //AutoSchedulerDataInputDataCounterDom.FullContainer.Dom.style.borderBottom = "none";
    //$(AutoSchedulerDataInputDataCounterDom.Input.Dom).addClass("InputBox");
    //$(AutoSchedulerDataInputDataCounterDom.Label.Dom).addClass("InputLabel");
    AutoSchedulerDataInputDataCounterDom.Label.Dom.style.width = "auto";
    AutoSchedulerDataInputDataCounterDom.Label.Dom.style.position = "relative";
    AutoSchedulerDataInputDataCounterDom.Label.Dom.style.marginRight = "3px";
    AutoSchedulerDataInputDataCounterDom.Label.Dom.style.display = "inline-block";
    $(AutoSchedulerDataInputDataCounterDom.Input.Dom).addClass(CurrentTheme.FontColor);
    AutoSchedulerDataInputDataCounterDom.Input.Dom.value = 1;
    AutoSchedulerDataInputDataCounterDom.Input.Dom.style.width = "2em";
    AutoSchedulerDataInputDataCounterDom.Input.Dom.style.height = "1em";
    AutoSchedulerDataInputDataCounterDom.Input.Dom.style.left = "auto"
    AutoSchedulerDataInputDataCounterDom.Input.setAttribute("type", "Number");
    //AutoSchedulerDataInputDataCounterDom.Input.Dom.style.left = "10%";
    //AutoSchedulerDataInputDataCounterDom.Input.Dom.style.top = 0;
    //AutoSchedulerDataInputDataCounterDom.Input.Dom.style.marginTop = 0;



    EventSplits = AutoSchedulerDataInputDataCounterDom.Input;

    //$(AutoSchedulerDataInputDataCounterDom.FullContainer).removClass("setAsDisplayNone");
    //$(DayPreferenceContainer.Dom).addClass("setAsDisplayNone");



    /*Recurrence Day Preference Container End*/
    EnabledRecurrenceContainer.Dom.appendChild(RecurrenceButtonContainer.Dom)
    EnabledRecurrenceContainer.Dom.appendChild(DaysOfTheWeekContainer.Dom);
    //EnabledRecurrenceContainer.Dom.appendChild(DayPreferenceContainer.Dom)

    if (!IsTile) {
        $(AutoSchedulerDataInputDataCounterDom.FullContainer).addClass("setAsDisplayNone");
        $(DayPreferenceContainer.Dom).addClass("setAsDisplayNone");
    }
    EnabledRecurrenceContainer.Dom.appendChild(RepetitionRangeCOntainer.Dom);


    RecurrenceTabContent.Dom.appendChild(EnabledRecurrenceContainer.Dom)
    RecurrenceTabContent.Dom.appendChild(AutoSchedulerDataInputDataCounterDom.FullContainer.Dom);
    RecurrenceTabContent.Dom.appendChild(DayPreferenceContainer.Dom)
    RecurrenceTabContent.Dom.appendChild(RecurrenceSectionCompletion.Dom);
    createDomEnablingFunction(AllDoms[3], 3, RecurrenceTabContent, DaysOfTheWeekContainer, DaysOfTheWeek.RevealDayOfWeek)();//defaults call to yearly










    function LaunchTab(MiscData) {
        var CurrentRange = MiscData[0].Content.getCurrentRangeOfAutoContainer();
        var i = 0;
        var NumbeOfValids = new Array();
        for (i; i < AllDoms.length; i++) {
            if (CurrentRange < AllDoms[i].Range) {
                RecurrenceButtonContainer.Dom.appendChild(AllDoms[i].Dom)
                NumbeOfValids.push(AllDoms[i]);
            }
        }

        var PercentageWidth = parseInt(100 / NumbeOfValids.length);

        for (i = 0; i < NumbeOfValids.length; i++)//takes advantage of order of AllDOms. Its arranged from smallest range to bigger
        {
            NumbeOfValids[i].Dom.style.width = (PercentageWidth - 1) + "%";
            NumbeOfValids[i].Dom.style.left = (i * PercentageWidth) + "%";
        }

        /*$(RecurrenceTabContent.Dom).addClass(CurrentTheme.ActiveTabContent);
        $(RecurrenceTabTitle.Dom).removeClass(CurrentTheme.AlternateFontColor);
        $(RecurrenceTabTitle.Dom).addClass(CurrentTheme.FontColor);
        $(RecurrenceTabTitle.Dom).addClass(CurrentTheme.ContentSection);

        $(RecurrenceTabTitle.Dom).removeClass(CurrentTheme.InActiveTabTitle);
        $(RecurrenceTabTitle.Dom).addClass(CurrentTheme.ActiveTabTitle);*/
    }

    function DisableTab(ExtraData) {
        /*$(RecurrenceTabContent.Dom).removeClass(CurrentTheme.ActiveTabContent);
        $(RecurrenceTabTitle.Dom).removeClass(CurrentTheme.ActiveTabTitle);
        $(RecurrenceTabTitle.Dom).removeClass(CurrentTheme.ContentSection);

        $(RecurrenceTabContent.Dom).addClass(CurrentTheme.InActiveTabContent);
        $(RecurrenceTabTitle.Dom).addClass(CurrentTheme.InActiveTabTitle);
        $(RecurrenceTabTitle.Dom).addClass(CurrentTheme.AlternateFontColor);*/
    }



    var retValue =
    {
        Name: "Recurrence Tab", Title: RecurrenceTabTitle, Content: RecurrenceTabContent, Completion: RecurrenceSectionCompletion, LaunchTab: LaunchTab, DisableTab: DisableTab
    };

    return retValue;
}

/*
function binds the date selector to the click event of the passed "LaunchDOm"
*/
function BindImputToDatePicketMobile(LaunchDOm) {

    LaunchDOm.onclick = function () {
        var Container = getDomOrCreateNew("ContainerDateElement");
        LaunchDatePicker(false, Container.Dom, LaunchDOm);
        Container.Dom.style.display = "block";
        CurrentTheme.getCurrentContainer().appendChild((Container.Dom));
    }


}

function createCalEventDoneTab() {
    var RangeTabContainerID = "RangeTabContainer";
    var RangeTabContainer = getDomOrCreateNew(RangeTabContainerID);
    var RangeTabTitleID = "RangeTabTitle";
    var RangeTabTitle = getDomOrCreateNew(RangeTabTitleID);
    var RangeTabTitleTextID = "RangeTabTitleText";
    var RangeTabTitleText = getDomOrCreateNew(RangeTabTitleTextID);
    RangeTabTitleText.Dom.innerHTML = "Done";
    var RangeTabContentID = "RangeTabContent";
    var RangeTabContent = getDomOrCreateNew(RangeTabContentID);
    RangeTabTitle.Dom.appendChild(RangeTabTitleText.Dom);
    $(RangeTabTitleText.Dom).addClass("TabTitleText");
    $(RangeTabTitleText.Dom).addClass(CurrentTheme.FontColor);
    $(RangeTabTitle.Dom).addClass("TabTitle");
    $(RangeTabContent.Dom).addClass("TabContent");

    RangeTabContainer.Dom.appendChild(RangeTabTitle.Dom);
    //RangeTabContainer.Dom.appendChild(RangeTabContent.Dom);

    var retValue = { Title: RangeTabTitle, Content: RangeTabContent, Button: RangeTabContainer };
    return retValue;
}


function createCalEventCancelTab() {
    var CancelTabContainerID = "CancelTabContainer";
    var CancelTabContainer = getDomOrCreateNew(CancelTabContainerID);
    var CancelTabTitleID = "CancelTabTitle";
    var CancelTabTitle = getDomOrCreateNew(CancelTabTitleID);
    var CancelTabTitleTextID = "CancelTabTitleText";
    var CancelTabTitleText = getDomOrCreateNew(CancelTabTitleTextID);
    //BackIcon


    CancelTabTitleText.Dom.innerHTML = "Cancel";
    var CancelTabContentID = "CancelTabContent";
    var CancelTabContent = getDomOrCreateNew(CancelTabContentID);
    CancelTabTitle.Dom.appendChild(CancelTabTitleText.Dom);
    $(CancelTabContent.Dom).addClass("BackIcon");
    $(CancelTabTitleText.Dom).addClass("TabTitleText");
    $(CancelTabTitleText.Dom).addClass(CurrentTheme.FontColor);
    $(CancelTabTitle.Dom).addClass("TabTitle");
    $(CancelTabContent.Dom).addClass("TabContent");

    CancelTabContainer.Dom.appendChild(CancelTabContent.Dom);
    CancelTabContainer.Dom.appendChild(CancelTabTitleText.Dom);


    var retValue = { Title: CancelTabTitle, Content: CancelTabContent, Button: CancelTabContainer };
    return retValue;
}






function generateDayOfWeekRepetitionDom() {
    var i = 0;
    var AllDoms = new Array();
    for (i = 0; i < WeekDays.length; i++) {
        AllDoms.push(createDoms(WeekDays[i], i));
    }

    var retValue = { AllDoms: AllDoms, RevealDayOfWeek: createRevealFunction(AllDoms) };

    function createDoms(DayOfWeek, index) {
        var DayDomID = DayOfWeek + "Recurrence";
        var DayDom = getDomOrCreateNew(DayDomID, "span");
        DayDom.Dom.innerHTML = "<p class=\"DayOfWeekLetter\">" + DayOfWeek[0] + "</p>";
        DayDom.DayOfWeekIndex = index;
        DayDom.Dom.style.left = ((index * 14.29) + (14.29 / 2)) + "%";
        //DayDom.Dom.style.marginLeft = "40px";
        $(DayDom.Dom).addClass("DayofWeekCircle");
        DayDom.status = 0;
        $(DayDom.Dom).click(generateFunctionForSelectedDayofWeek(DayDom));
        generateFunctionForSelectedDayofWeek(DayDom);
        function generateFunctionForSelectedDayofWeek(DayDom) {
            return function () {
                DayDom.status += 1;
                DayDom.status %= 2;
                switch (DayDom.status) {
                    case 0:
                        {
                            $(DayDom.Dom).removeClass("SelectedDayOfWeek");
                            $(DayDom.Dom).addClass("deSelectedDayOfWeek");
                        }
                        break;
                    case 1:
                        {
                            $(DayDom.Dom).removeClass("deSelectedDayOfWeek");
                            $(DayDom.Dom).addClass("SelectedDayOfWeek");
                        }
                        break;
                }
            }
        }

        return DayDom;
    }


    function createRevealFunction(AllDoms) {//creates function that slowly displays the day of the week circles
        return function () {
            var i = 0;
            AllDoms.forEach(function (myDom) {
                setTimeout(function () { myDom.Dom.style.opacity = 1; myDom.Dom.style.top = "0px"; }, i * 200);
            }
            )
        }
    }

    return retValue;
}





function LaunchDatePicker(isRigid, Container, DivWithDateData) {
    var DatePickerOptionContainer = getDomOrCreateNew("DatePickerOptionContainer");
    var DatePickerTitleContainer = getDomOrCreateNew("DatePickerTitle", "span");
    DatePickerTitleContainer.Dom.innerHTML = "Select an Option";
    var DatePickerButtonContainer = getDomOrCreateNew("DatePickerButtonContainer");
    DatePickerButtonContainer = getDomOrCreateNew("DatePickerButtonContainer");
    DivWithDateData.blur();
    var SelectDateButtonContainer = getDomOrCreateNew("SelectDateContainer");

    var SelectDateButton = getDomOrCreateNew("SelectDate", "input");
    SelectDateButton.Dom.value = DivWithDateData.value
    SelectDateButtonContainer.Dom.appendChild(SelectDateButton.Dom);

    $(DatePickerTitleContainer.Dom).addClass(CurrentTheme.AlternateFontColor);

    if (!isRigid) {
        var AllButtonsContainer = getDomOrCreateNew("AllButtonsContainer");
        var EndOfTodaybutton = getDomOrCreateNew("EndOfToday", "button");
        EndOfTodaybutton.Dom.innerHTML = "Today";
        var EndOfTomorrowButton = getDomOrCreateNew("EndOfTomorrow", "button");
        EndOfTomorrowButton.Dom.innerHTML = "Tomorrow";
        var EndOfWeekbutton = getDomOrCreateNew("EndOfWeek", "button");
        EndOfWeekbutton.Dom.innerHTML = "End of the Week(Sat)";
        var EndOfMonthButton = getDomOrCreateNew("EndOfMonth", "button");
        EndOfMonthButton.Dom.innerHTML = "End of the Month";

        $(EndOfTodaybutton.Dom).addClass("DateSelectionButton");
        $(EndOfTomorrowButton.Dom).addClass("DateSelectionButton");
        $(EndOfWeekbutton.Dom).addClass("DateSelectionButton");
        $(EndOfMonthButton.Dom).addClass("DateSelectionButton");



        $(EndOfTodaybutton.Dom).click(function () { var currentDateData = getEndOfTodayDateTime(); populateTextBox(currentDateData, DivWithDateData, Container) });
        $(EndOfTomorrowButton.Dom).click(function () { var currentDateData = getEndOfTomorrowDateTime(); populateTextBox(currentDateData, DivWithDateData, Container) });
        $(EndOfWeekbutton.Dom).click(function () { var currentDateData = getEndOfTheWeekDateTime(); populateTextBox(currentDateData, DivWithDateData, Container) });
        $(EndOfMonthButton.Dom).click(function () { var currentDateData = getEndOfTheMontDateTime(); populateTextBox(currentDateData, DivWithDateData, Container) });



        AllButtonsContainer.Dom.appendChild(EndOfTodaybutton.Dom);
        AllButtonsContainer.Dom.appendChild(EndOfTomorrowButton.Dom);
        AllButtonsContainer.Dom.appendChild(EndOfWeekbutton.Dom);
        AllButtonsContainer.Dom.appendChild(EndOfMonthButton.Dom);
        DatePickerButtonContainer.Dom.appendChild(AllButtonsContainer.Dom);
    }



    DatePickerButtonContainer.Dom.appendChild(DatePickerTitleContainer.Dom);
    DatePickerButtonContainer.Dom.appendChild(SelectDateButtonContainer.Dom);
    $(DatePickerButtonContainer.Dom).addClass(CurrentTheme.AlternateContentSection);

    DatePickerOptionContainer.Dom.appendChild(DatePickerButtonContainer.Dom);


    $(DatePickerOptionContainer.Dom).click(function (event) { if (event.target == DatePickerOptionContainer.Dom) populateTextBox(null, DivWithDateData, Container) });

    Container.appendChild(DatePickerOptionContainer.Dom);
    $(SelectDateButton.Dom).datebox({
        mode: "slidebox",
        //afterToday: true,
        overrideDateFormat: "%m/%d/%Y",
        closeCallback: function () {
            if (SelectDateButton.Dom.value == "") {
                return;
            }
            var selectedDate = SelectDateButton.Dom.value;
            var selectedDate_split = selectedDate.split("/");
            var retValue;

            if (!(selectedDate) || (selectedDate_split.length < 1)) {
                retValue = null;
            }
            else {
                retValue = { Month: selectedDate_split[0], Day: selectedDate_split[1], Year: selectedDate_split[2] };
            }
            populateTextBox(retValue, DivWithDateData, Container);



        }
    });

    SelectDateButton.onclick = function () {
        var aElement2 = $(SelectDateButton.parentElement).children("a");
        aElement2[0].click()
    };

    function handler(event)//prevents code from propagating to parent(DatePickerOptionContainer)
    {
        event.stopPropagation();
    }



    //$(DatePickerButtonContainer.Dom).add(DatePickerOptionContainer).click(handler);

}

function populateTextBox(DateData, container, ContainerDateElement) {
    if (DateData != null) {
        container.value = DateData.Month + "/" + DateData.Day + "/" + DateData.Year;
    }
    ContainerDateElement.style.display = "none";
    $(ContainerDateElement).empty();
}

function getEndOfTodayDateTime() {
    var RetValue = new Date(Date.now());
    RetValue.setHours(23, 59, 59);

    var retValue = { Month: RetValue.getMonth() + 1, Day: RetValue.getDate(), Year: RetValue.getFullYear() };
    return retValue;
}

function getEndOfTheWeekDateTime() {
    var RetValue = new Date(Date.now());
    RetValue.setHours(0, 0, 0, 0);
    var currDay = RetValue.getDay();
    var Multiplier = 6 - currDay;

    RetValue = new Date(RetValue.getTime() + (Multiplier * OneDayInMs));
    var retValue = { Month: RetValue.getMonth() + 1, Day: RetValue.getDate(), Year: RetValue.getFullYear() };
    return retValue;
}

function getEndOfTheMontDateTime() {
    var RetValue = new Date(Date.now());
    var nextMonth = (RetValue.getMonth() + 1) % 12;
    var year = RetValue.getFullYear();
    if (nextMonth == 0) {
        year + 1;
    }
    RetValue = new Date(new Date(year, nextMonth, 1, 23, 59, 59).getTime() - OneDayInMs);

    var retValue = { Month: RetValue.getMonth() + 1, Day: RetValue.getDate(), Year: RetValue.getFullYear() };
    return retValue;
}



function getEndOfTomorrowDateTime() {
    var RetValue = new Date(Date.now());
    RetValue = new Date(RetValue.getTime() + OneDayInMs);
    RetValue.setHours(23, 59, 59);
    var retValue = { Month: RetValue.getMonth() + 1, Day: RetValue.getDate(), Year: RetValue.getFullYear() };
    return retValue;
}

function GenerateColorPickerContainer(OverLay) {
    var pickColorButton = getDomOrCreateNew("SelectColorButton");
    $(pickColorButton.Dom).click(generateColorPicker);
    var DarkOverLay = getDomOrCreateNew("DarkOverlay");
    function loopBackFunction() {
        /*
        setTimeout(
        function () {*/
        $(pickColorButton.Dom).addClass(myPicker.Selector.getColor().cssClass);
        $(myPicker.Selector.Container).remove()
        $(DarkOverLay.Dom).remove();

        //}, 500);
    }



    var myPicker = generateColorPickerContainer(loopBack);
    function loopBack(SelectedColorData) {
        if (loopBack.previousClass != null) {
            $(pickColorButton.Dom).removeClass(loopBack.previousClass);
        }
        $(pickColorButton.Dom).addClass(SelectedColorData.ColorClass);
        loopBack.previousClass = SelectedColorData.ColorClass;
    }
    loopBack.previousClass = null;




    function generateColorPicker() {
        //DarkOverLay = getDomOrCreateNew("DarkOverlay");

        $(myPicker.Selector.Container).click(function (event) {//stops clicking of add event from propagating
            event.stopPropagation();
        });


        DarkOverLay.Dom.appendChild(myPicker.Selector.Container)

        CurrentTheme.getCurrentContainer().appendChild((DarkOverLay.Dom));

        //OverLay.appendChild(DarkOverLay.Dom);
        $(DarkOverLay.Dom).click(loopBackFunction);
    }

    var retValue = { Button: pickColorButton.Dom, Picker: myPicker };

    return retValue;

}




