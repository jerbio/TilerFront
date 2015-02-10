"use strict";

function addNewEvent(x, y, height, refStart)
{
    //debugger;
    var AddEventPanel = getDomOrCreateNew("AddEventPanel");
    generateAddEventContainer(x, y, height, AddEventPanel.Dom, refStart);
    
}


function prepSendTile(NameInput, AddressInput, SpliInput, HourInput, MinuteInput, DeadlineInput, RepetitionInput, RepetitionFlag)
{
    return function ()
    {
        var calendarColor = global_AllColorClasses[0];
        
        SubmitTile(NameInput.value, AddressInput.value, SpliInput.value, HourInput.value, MinuteInput.value, DeadlineInput.value, RepetitionInput.value, calendarColor, RepetitionFlag);
    }
}

function SubmitTile(Name, Address, Splits, Hour, Minutes, Deadline, Repetition, CalendarColor,RepetitionFlag)
{
    var DictOfData = {};
    DictOfData["day"] = { Range: OneDayInMs, Type: { Name: "Daily", Index: 0 }, Misc: null }
    DictOfData["week"] = { Range: OneWeekInMs, Type: { Name: "Weekly", Index: 1 }, Misc: { AllDoms: [] } }
    DictOfData["month"] = { Range: FourWeeksInMs, Type: { Name: "Monthly", Index: 2 }, Misc: null }
    DictOfData["year"] = { Range: OneYearInMs, Type: { Name: "Yearly", Index: 3 }, Misc: null }


    var EventName = Name;
    if (!EventName)
    {
        alert("Oops your tile needs a name");
        return null;
    }
    var LocationAddress = Address;
    var LocationNickName = "";
    var EventLocation = new Location(LocationNickName, LocationAddress);
    Hour = Hour != "" ? Hour : 0;
    Minutes = Minutes != "" ? Minutes : 0;

    
    var Start = new Date();
    var EventStart = {}
    EventStart.Date = new Date(Start.getFullYear(), Start.getMonth(), Start.getDate());
    EventStart.Time = { Hour: 0, Minute: 0 };
    var End = new Date(Deadline);
    

    var EventDuration = { Days: 0, Hours: Hour, Mins: Minutes };

    var DurationInMS = (parseInt(EventDuration.Days) * OneDayInMs) + (parseInt(EventDuration.Hours) * OneHourInMs) + (parseInt(EventDuration.Mins) * OneMinInMs)
    if (DurationInMS == 0) {
        alert("Oops please provide a duration for \"" + EventName + "\"");
        return null;
    }

    Splits = Splits != "" ? Splits : 1;
    
    Repetition = Repetition.trim().toLowerCase();
    var DayPlusOne = new Date();
    var Day = DayPlusOne.getDate();
    var Month = DayPlusOne.getMonth() + 1;
    var Year = DayPlusOne.getFullYear();
    var DatePickerValue = Month + "/" + Day + "/" + Year;

    var RepetitionStart = DatePickerValue;
    var RepetitionEnd = ""


    var repeteOpitonSelect = "none"
    if( (Repetition != "")&&(RepetitionFlag))
    {
        repeteOpitonSelect = DictOfData[Repetition];
        if (repeteOpitonSelect != undefined) {
            RepetitionEnd = (End.getMonth() + 1) + "/" + End.getDate() + "/" + End.getFullYear();
            var FullRange = End.getTime() - EventStart.Date.getTime()
            if (repeteOpitonSelect.Range > FullRange)//checks if the given deadline extends past the range for a selected repetition sequence. e.g If user selects weekly, this line checks if range is between start and end is larger than 7 days
            {
                alert("please check your repetition, you dont have up to a " + Repetition + " before deadline");
                return;
            }

            End = new Date(Start.getTime() + repeteOpitonSelect.Range);
        }
        else
        {
            alert("Seems like you have invalid data for repetition. Please check your repetition");
            return;
        }
        
    }
    var EventEnd = {}
    EventEnd.Date = new Date(End.getFullYear(), End.getMonth(), End.getDate());
    EventEnd.Time = { Hour: 23, Minute: 59 };
    
    var NewEvent = new CalEventData(EventName, EventLocation, Splits, CalendarColor, EventDuration, EventStart, EventEnd, repeteOpitonSelect, RepetitionStart, RepetitionEnd, false);
    //NewEvent.RepeatData = null;
    if (NewEvent == null) {
        return;
    }
    NewEvent.UserName = UserCredentials.UserName
    NewEvent.UserID = UserCredentials.ID;

    var TimeZone = new Date().getTimezoneOffset();
    NewEvent.TimeZoneOffset = TimeZone;
    //var url = "RootWagTap/time.top?WagCommand=1"
    var url = global_refTIlerUrl + "Schedule/Event";

    var HandleNEwPage = new LoadingScreenControl("Tiler is Adding \"" + NewEvent.Name + " \" to your schedule ...");
    //alert("about to send out");
    //debugger;
    //return;
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
            //var myContainer = (CurrentTheme.getCurrentContainer());
            //CurrentTheme.TransitionOldContainer();
            //$(myContainer).empty();
            //myContainer.outerHTML = "";
        },
        error: function (err) {
            //var myError = err;
            //var step = "err";
            var NewMessage = "Oh No!!! Tiler is having issues modifying your schedule. Please try again Later :(";
            var ExitAfter = { ExitNow: true, Delay: 1000 };
            HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, function () { });
        }

    }).done(function (data) {
        HandleNEwPage.Hide();
        AddTiledEvent.Exit();
        getRefreshedData();
        

    });

}


function generateModal(x, y, height, width,WeekStart, RenderPlane)
{
    //return;

    var modalAddDom = getDomOrCreateNew("AddModalDom");
    
    var weekDayWidth = $($(".DayContainer")[0]).width();
    var AddTile = getDomOrCreateNew("AddTileDom", "button");
    var AddEvent = getDomOrCreateNew("AddEventDom", "button");
    AddEvent.Dom.innerHTML=("Add New Event");
    AddTile.Dom.innerHTML=("Add New Tile");
    modalAddDom.Dom.appendChild(AddEvent.Dom);
    modalAddDom.Dom.appendChild(AddTile.Dom);
    $(AddTile.Dom).addClass("ModalButton");
    $(AddEvent.Dom).addClass("ModalButton");

    modalAddDom.Dom.style.left = x + "px";
    modalAddDom.Dom.style.top = y + "px";
    if (AddTiledEvent.Exit != undefined)
    {
        AddTiledEvent.Exit();
    }
    $(AddEvent.Dom).click(function () {
        (modalAddDom.Dom.parentElement.removeChild(modalAddDom.Dom));
        var floatalTime = y / height;
        var Hour = Math.floor((floatalTime) * 24);
        var Min = 0;
        var WeekDayIndex = Math.floor(x / weekDayWidth);
        var myDate = new Date(WeekStart);
        var NewDay = myDate.getDate() + WeekDayIndex
        myDate.setDate(NewDay);
        myDate.setHours(Hour);
        myDate.setMinutes(0);;
        addNewEvent(x, y, height, myDate);
    });

    $(AddTile.Dom).click(function ()
    {
        (modalAddDom.Dom.parentElement.removeChild(modalAddDom.Dom));
        var modalData = AddTiledEvent();
        //RenderPlane.appendChild(modalData.Dom);
    });


    $(AddEvent.Dom).click(function (event) {//stops clicking of add event button from triggering a new modal dom
        event.stopPropagation();
    });

    $(AddTile.Dom).click(function (event) {//stops clicking of add event button from triggering a new modal dom
        event.stopPropagation();
    });

    function removePanel(e) {
        if (e.which == 27) {
            $(document).off("keydown", document, removePanel);
            CloseModal();
        }
        e.stopPropagation();
    }

    
    modalAddDom.Dom.onblur = function ()//closes modal add when the modal panels is out of focus
    {
        function isDescendant(parent, child) {
            var node = child.parentNode;
            while (node != null) {
                if (node == parent) {
                    return true;
                }
                node = node.parentNode;
            }
            return false;
        }
        
        setTimeout(function () {
            if (!isDescendant(modalAddDom, document.activeElement)) {
                $(document).off("keydown", document, removePanel);
                CloseModal();
            }
        }, 1);

        

        
    };
    $(document).keydown(removePanel);
    RenderPlane.appendChild(modalAddDom.Dom);
    $(modalAddDom.Dom).attr('tabindex', 0).focus();

}


function CloseModal()
{
    var myAddPanel = getDomOrCreateNew("AddModalDom");
    if (myAddPanel.Dom.parentElement != null)
    {
        myAddPanel.Dom.parentElement.removeChild(myAddPanel.Dom);
    }

}

function generateAddEventContainer(x,y,height,Container,refStartTime)
{
    getRefreshedData.isEnabled = false;
    ActivateUserSearch.setSearchAsOff();
    var NewEventcontainer = getDomOrCreateNew("AddNewEventContainer");

    $(NewEventcontainer.Dom).click(function (event) {//stops clicking of add event from propagating
        event.stopPropagation();
    });
    function removePanel(e)
    {
        if (e.keyCode == 27)
        {
            getRefreshedData();
            CloseEventAddition()
            
        }
        e.stopPropagation();
    }
    

    function CloseEventAddition()
    {
        $(document).off("keyup", document, removePanel);
        if (NewEventcontainer != null) {
            if (NewEventcontainer.Dom.parentElement != null) {
                NewEventcontainer.Dom.parentElement.removeChild(NewEventcontainer.Dom);
            }
        }
        ActivateUserSearch.setSearchAsOn();
    }
    $(document).keyup(removePanel);

    //myClickManager.AddNewElement(NewEventcontainer.Dom);
    //NewEventcontainer.Dom.style.left = x+"px";
    //NewEventcontainer.Dom.style.top = y + "px";
    Container.appendChild(NewEventcontainer.Dom);
    var NameDom = generateNameContainer();
    var LocationDom = generateLocationContainer();
    var DurationDom = generateDurationSliderContainer();
    var StartDom = generateStartContainer(refStartTime);
    var EndDom = generateEndContainer();
    var SubmitButton = generateSubmitButton();
    var SplitCount = splitInputText();
    var ColorPicker = generateColorPickerContainer()
  //  var EnableTiler = generateTilerEnabled(EndDom.Selector.Container, SplitCount.Selector.Container);

    NewEventcontainer.Dom.appendChild(NameDom.Selector.Container);
    NewEventcontainer.Dom.appendChild(StartDom.Selector.Container);
    NewEventcontainer.Dom.appendChild(DurationDom.Selector.Container);
    //NewEventcontainer.Dom.appendChild(EndDom.Selector.Container);
    NewEventcontainer.Dom.appendChild(LocationDom.Selector.Container);
    NewEventcontainer.Dom.appendChild(ColorPicker.Selector.Container);
    //NewEventcontainer.Dom.appendChild(EnableTiler.Selector.Container);
    NewEventcontainer.Dom.appendChild(SubmitButton.Selector.Container);
    //NewEventcontainer.Dom.appendChild(SplitCount.Selector.Container);
    
    
    
    

    $(SubmitButton.Selector.Button.Dom).click(function () {
        BindSubmitClick(NameDom.Selector.Input.Dom.value, LocationDom.Selector.Address.Dom.value, LocationDom.Selector.NickName.Dom.value, SplitCount.Selector.Input.Dom.value, StartDom, EndDom, DurationDom, null, true, ColorPicker.Selector.getColor(), CloseEventAddition)
    })
    //var RepetitionDom = generateRepetitionContainer();
}

function generateNameContainer()
{
    var NameContainer = getDomOrCreateNew("NameContainer");
    var InputContainer = getDomOrCreateNew("NameInput", "input");
    var EscapeMessage = getDomOrCreateNew("EscapeMessage", "span");
    InputContainer.Dom.setAttribute("placeholder", "Name Of Event");
    InputContainer.Dom.setAttribute("autofocus", true);
    EscapeMessage.Dom.innerHTML = "Press Escape to escape";
    NameContainer.Dom.appendChild(InputContainer.Dom);
    NameContainer.Dom.appendChild(EscapeMessage.Dom);
    NameContainer.Selector = { Container: NameContainer.Dom, Input: InputContainer };
    return NameContainer;
}

function generateLocationContainer()
{
    var LocationContainer = getDomOrCreateNew("LocationContainer");
    var LocationInputContainer = getDomOrCreateNew("LocationInput", "input");
    var NickLocationInputContainer = getDomOrCreateNew("NickLocationInput", "input");
    LocationContainer.Dom.appendChild(LocationInputContainer.Dom);
    LocationInputContainer.Dom.setAttribute("placeholder", "Address?");
    // LocationContainer.Dom.appendChild(NickLocationInputContainer.Dom);
    //  NickLocationInputContainer.Dom.setAttribute("placeholder", "Nick Name");
    LocationContainer.Selector = { Container: LocationContainer.Dom, NickName: NickLocationInputContainer, Address: LocationInputContainer };
    return LocationContainer;
}

function generateDurationSliderContainer()
{
    var DurationExplanation = getDomOrCreateNew("DurationExplanation", "span");
    DurationExplanation.Dom.innerHTML = "<p>Duration of the event</p>";
    var DurationContainer = getDomOrCreateNew("DurationContainer");
    var HourSliderValue = getDomOrCreateNew("HourSliderValue","input");
    HourSliderValue.Dom.value=0;
    var HourLabel = getDomOrCreateNew("HourLabel", "span");
    HourLabel.Dom.innerHTML="H";
    var MinSliderValue = getDomOrCreateNew("MinSliderValue", "input");
    MinSliderValue.Dom.value = 0;
    var MinLabel = getDomOrCreateNew("MinLabel", "span");
    MinLabel.Dom.innerHTML="M";
    var DaySliderValue = getDomOrCreateNew("DaySliderValue", "input");
    DaySliderValue.Dom.value = 0;
    var DayLabel = getDomOrCreateNew("DayLabel", "span");
    DayLabel.Dom.innerHTML="D"
    DurationContainer.Selector = {};


    DurationContainer.Dom.appendChild(DurationExplanation.Dom);
    DurationContainer.Dom.appendChild(HourSliderValue.Dom);
    DurationContainer.Dom.appendChild(HourLabel.Dom);
    DurationContainer.Dom.appendChild(MinSliderValue.Dom);
    DurationContainer.Dom.appendChild(MinLabel.Dom);
    DurationContainer.Dom.appendChild(DaySliderValue.Dom);
    DurationContainer.Dom.appendChild(DayLabel.Dom);

    var TimeHolder = function ()
    {
        return { Days: DaySliderValue.Dom.value, Hours: HourSliderValue.Dom.value, Mins: MinSliderValue.Dom.value }
    }
    DurationContainer.Selector = { Container: DurationContainer.Dom, Minute: MinSliderValue, Hour: HourSliderValue, Day: DaySliderValue, TimeHolder: TimeHolder };
    return DurationContainer;
}

function generateStartContainer(refDate)
{
    var StartDateTimeContainer = getDomOrCreateNew("StartTimeContainer");
    var StartTimeInputContainer= getDomOrCreateNew("StartTimeInputContainer");
    var StartTimeInput = getDomOrCreateNew("StartTimeInput", "Input");
    StartTimeInput.Dom.setAttribute("placeholder", "Start Time(Default Now)");
    var StartDateInputContainer = getDomOrCreateNew("StartDateInputContainer");
    var StartDateInput = getDomOrCreateNew("StartDateInput", "Input");
    var CurrentDate = new Date();
    var CurrentDate = FormatTime(CurrentDate);
    var CurrentDate = CurrentDate.month_num+1 + '/' + CurrentDate.date + '/' + CurrentDate.year;

    StartDateInput.Dom.setAttribute("placeholder", CurrentDate);

    StartTimeInputContainer.Dom.appendChild(StartTimeInput.Dom);
    StartDateInputContainer.Dom.appendChild(StartDateInput.Dom);
    BindDatePicker(StartDateInput.Dom);
    BindTimePicker(StartTimeInput.Dom);

    StartTimeInput.Dom.value = getTimeStringFromDate(refDate);
    var currDate = getDateString(refDate);
    StartDateInput.Dom.value = currDate;
    StartDateTimeContainer.getDateTimeData = getFullTimeFromEntry(StartTimeInput, StartDateInput, 0);

    StartDateTimeContainer.Dom.appendChild(StartTimeInputContainer.Dom);
    StartDateTimeContainer.Dom.appendChild(StartDateInputContainer.Dom);
    StartDateTimeContainer.Selector = { Container: StartDateTimeContainer.Dom, Time: StartTimeInputContainer, Date: StartDateInputContainer };
    return StartDateTimeContainer;
}

function generateTilerEnabled(EndTimeContainer,SplitContainer)
{
    var ButtonID = "SlideButton";
    var Button = generateMyButton(ResilveSLider,ButtonID);
    //Button.status = 1;
    var EnableSplitAndEndContainer = "EnableSplitAndEndContainer";
    EnableSplitAndEndContainer = getDomOrCreateNew(EnableSplitAndEndContainer);
    EnableSplitAndEndContainer.Dom.appendChild(EndTimeContainer);
    EnableSplitAndEndContainer.Dom.appendChild(SplitContainer);
    
    $(EnableSplitAndEndContainer.Dom).addClass("DisableTiler");
    function ResilveSLider()//call back function to be triggered with the button slider
    {
        if (Button.status == 1)
        {
            $(EnableSplitAndEndContainer.Dom).addClass("DisableTiler"); 
        }
        else
        {
            $(EnableSplitAndEndContainer.Dom).removeClass("DisableTiler");
        }
    }

    var EnableTIlerContainer = "EnableTilerContainer";
    EnableTIlerContainer=getDomOrCreateNew(EnableTIlerContainer);
    EnableTIlerContainer.Dom.appendChild(Button.Dom);
    EnableTIlerContainer.Dom.appendChild(EnableSplitAndEndContainer.Dom);

    
    EnableTIlerContainer.Selector = { Container: EnableTIlerContainer.Dom, Button: Button };
    return EnableTIlerContainer;
}


function AddToTileContainer(TileInptObject, Container) {
    //debugger;
    var AllElements = TileInptObject.getAllElements();
    for (var i = 0; i < AllElements.length; i++) {
        //debugger;
        var myElement = AllElements[i];
        if (myElement != null)
        {
            Container.Dom.appendChild(myElement);
        }
    }
}

//Handles the activities of sliders. Sliders show up beneath the done button
function InactiveSlider(InActiveDom, ActiveDom,ButtonElements)
{
    var InactiveSliderID =  InactiveSlider.ID++;
    var ButtonSlide = generateMyButton(LoopBackFunction);
    var InActiveMessage = ButtonElements.InActiveMessage;
    var InActiveMessageContainer = getDomOrCreateNew("InActiveMessage" + InactiveSliderID, "span");
    InActiveMessageContainer.Dom.innerHTML = InActiveMessage;
    var ActiveMessage = ButtonElements.ActiveMessage;
    var ActiveMessageContainer = getDomOrCreateNew("ActiveMessage" + InactiveSliderID, "span");
    ActiveMessageContainer.Dom.innerHTML = ActiveMessage;

    $(ActiveMessageContainer.Dom).addClass("HideInactiveElement");
    $(InActiveMessageContainer.Dom).addClass("HideInactiveElement");

    var SliderMessageContainer = getDomOrCreateNew("SliderMessageContainer" + InactiveSliderID);
    SliderMessageContainer.Dom.appendChild(ActiveMessageContainer.Dom)
    SliderMessageContainer.Dom.appendChild(InActiveMessageContainer.Dom)
    $(SliderMessageContainer.Dom).addClass("SliderMessageContainer");

    var ActiveContainerID = "ActiveSliderContainer" + InactiveSliderID;

    var ActiveContainer = getDomOrCreateNew(ActiveContainerID);//Container for All Slider Data. In deactivated mode it shows only deactivated message
  
    $(ActiveContainer.Dom).addClass("ActiveContainerSlider");
    var AllInputData = ButtonElements.ButtonElements;
    ActiveContainer.Dom.appendChild(ButtonSlide.Dom);
    ActiveContainer.Dom.appendChild(SliderMessageContainer.Dom);
    
    var AlInputDataContainerID = "AlInputDataContainerID" + InactiveSliderID;
    var AllInputDataContainer = getDomOrCreateNew(AlInputDataContainerID);
    ActiveContainer.Dom.appendChild(AllInputDataContainer.Dom);
    $(AllInputDataContainer.Dom).addClass("HideInactiveElement");

    var AllTileElements = [];//Stores TileInputBox objects
    var LastElement = new TileInputBox(AllInputData[AllInputData.length - 1], AllInputDataContainer, undefined, AddTiledEvent.Exit);
    AllTileElements.push(LastElement);

    for (var i = AllInputData.length - 2, j = AllInputData.length - 1; i >= 0; i--, j--)
    {
        AllInputData[i].NextElement = LastElement;
        LastElement = new TileInputBox(AllInputData[i], AllInputDataContainer, undefined, AddTiledEvent.Exit);
        AllTileElements.push(LastElement);
    }

    var FirstElement = LastElement;
    while (LastElement.NextElement != undefined)
    {
        AddToTileContainer(LastElement, AllInputDataContainer);
        //AllInputDataContainer.Dom.appendChild(LastElement.FullContainer.Dom);
        LastElement = LastElement.NextElement;
    }

    AddToTileContainer(LastElement, AllInputDataContainer);
    //AllInputDataContainer.Dom.appendChild(LastElement.FullContainer.Dom);

    this.getAllElements =function()
    {
        return AllInputData;
    }

    
    FirstElement.reveal();

    function LoopBackFunction()
    {
        if (ButtonSlide.status == 0)
        {
            Deactivate();
        }
        else
        {
            Activate();
            //debugger;

            //var AllDoms = $(ButtonSlide).next("input")
            //$(ButtonSlide).next("div").next().children("input")[0].focus()
            ButtonSlide.blur()
            var InputDom = $(ButtonSlide).next("div").next().children("input")[0];
            setTimeout(function () { InputDom.focus(); }, 0)
            
            ButtonSlide.blur()
            
        }
    }

    function ShowMessage()
    {
        var message = getDomOrCreateNew("ButtonMessage" + InactiveSliderID, "Span");
        message.innerHTML = "Press Spacebar to activate";
        $(message).addClass('pressSpacebarSpan');
        ButtonSlide.Dom.parentElement.appendChild(message);
        $(ButtonSlide.Dom.parentElement).addClass('spanParent');
        $(message).removeClass("HideInactiveElement");
    }
    function HideMessage()
    {
        var message = getDomOrCreateNew("ButtonMessage" + InactiveSliderID, "Span");
        $(message).addClass("HideInactiveElement");
        $(message).removeClass('pressSpacebarSpan');
    }

    ButtonSlide.Dom.addEventListener("focus", ShowMessage);
    ButtonSlide.Dom.addEventListener("blur", HideMessage);

    function Activate()
    {
        $(ActiveMessageContainer.Dom).addClass("RevealInactiveElement");
        $(ActiveMessageContainer.Dom).removeClass("HideInactiveElement");

        $(InActiveMessageContainer.Dom).addClass("HideInactiveElement");
        $(InActiveMessageContainer.Dom).removeClass("RevealInactiveElement");

        $(AllInputDataContainer.Dom).addClass("AlInputDataContainer");
        $(AllInputDataContainer.Dom).removeClass("HideInactiveElement");
        InActiveDom.removeOptions(ActiveContainer.Dom)
        ActiveDom.addOptions(ActiveContainer.Dom);
    }

    function Deactivate()
    {
        
        $(InActiveMessageContainer.Dom).addClass("RevealInactiveElement");
        $(InActiveMessageContainer.Dom).removeClass("HideInactiveElement");

        $(ActiveMessageContainer.Dom).addClass("HideInactiveElement");
        $(ActiveMessageContainer.Dom).removeClass("RevealInactiveElement");

        $(AllInputDataContainer.Dom).removeClass("AlInputDataContainer");
        $(AllInputDataContainer.Dom).addClass("HideInactiveElement");
        ActiveDom.removeOptions(ActiveContainer.Dom)
        InActiveDom.addOptions(ActiveContainer.Dom);
        

    }

    this.getStatus = function ()
    {
        return ButtonSlide.status;
    }

    ButtonSlide.SetAsOff();
}

InactiveSlider.ID = 0;




function PopulateSliders(AcitveSection, InAcitveSection)
{
    var RepetionSliderData = GenerateTileRepetition();
    var RepetitionSlider = new InactiveSlider(InAcitveSection.Dom,AcitveSection.Dom, RepetionSliderData);

    return RepetitionSlider;
}

function GenerateTileRepetition()
{
    var CountElementData = { LabelBefore: "I need to do this" };
    var PerElementData = { LabelBefore: "times per", DefaultText: "Day/Week/Month/Year", DropDown: { url: [{ repetition: "Day" }, { repetition: "Week" }, { repetition: "Month" }, { repetition: "Year" }, { repetition: "Decade" }], LookOut: "repetition" } };

    var ButtonElements = [];
    ButtonElements.push(CountElementData);
    ButtonElements.push(PerElementData);
    var InActiveMessage = "Repeatedly? Currently: No";
    var ActiveMessage = "Repeatedly";
    var RetValue = { InActiveMessage: InActiveMessage, ActiveMessage: ActiveMessage, ButtonElements: ButtonElements }
    return RetValue;
}



//handles the whole addition of tiled events. Handles the UI component and tabbing
function AddTiledEvent()
{
    getRefreshedData.isEnabled = false;
    ActivateUserSearch.setSearchAsOff();
    var InvisiblePanelID = "AddEventPanel";
    var InvisiblePanel = getDomOrCreateNew(InvisiblePanelID);
    //$(InvisiblePanel.Dom).addClass("InvisibleAddEventPanel");
    var AllInputData = new Array()


    var ModalTileContainerID = "ModalTileContainer";
    var modalTileEvent = getDomOrCreateNew(ModalTileContainerID);//full container with done bar, Active container and Sub Slider;

    var ActiveSectionID = "ModalActiveTileContainer";
    var ActiveContainer = getDomOrCreateNew(ActiveSectionID);
    var InActiveSectionID = "ModalInActiveTileContainer";
    var InActiveContainer = getDomOrCreateNew(InActiveSectionID);
    


    $(modalTileEvent.Dom).addClass("ModalTileContainer");

    var ModalContentContainerID = "ModalContentContainer";
    var ModalContentContainer = getDomOrCreateNew(ModalContentContainerID);//contains the current content
    var ModalDoneContainerID = "ModalDoneContentContainer";
    var ModalDoneContentContainer = getDomOrCreateNew(ModalDoneContainerID);//Contains the done section
    var ModalActiveOptionsContainerID = "ModalActiveOptionsContainer"
    var ModalActiveOptionsContainer = getDomOrCreateNew(ModalActiveOptionsContainerID);//Contains the options when turned on

    ActiveContainer.Dom.appendChild(ModalContentContainer.Dom);
    ActiveContainer.Dom.appendChild(ModalActiveOptionsContainer.Dom)
    ActiveContainer.Dom.appendChild(ModalDoneContentContainer.Dom)
    

    ModalActiveOptionsContainer.addOptions = function (NewOption)
    {
        ModalActiveOptionsContainer.appendChild(NewOption.Dom);
    }

    function sentenceCompletion()
    {
        var ModalSenetenceContainerID = "ModalSenetenceContainer";
        var ModalSenetenceContainer = getDomOrCreateNew(ModalSenetenceContainerID);
        var FullSentenceContentID = "FullSentenceContent"
        var FullSentenceContent = getDomOrCreateNew(FullSentenceContentID);
        ModalSenetenceContainer.appendChild(FullSentenceContent);
        hideAutoSentence();
        var Messages = {};
        var orderedMessage =[]
        function addTileInput(tileInput)
        {
            Messages[tileInput.Message.Index] = tileInput;
            orderedMessage.push(tileInput);
            orderedMessage.sort(
                function (a, b)
                {
                    var retvalue = (a.Message.Index) - (b.Message.Index);
                    return retvalue;
                });
            //orderedMessage.reverse();
        }

        this.addTileInput = addTileInput;
        function updateSentence()
        {
            var fullMessage = "";
            for (var i = 0 ; i < orderedMessage.length; i++)
            {
                fullMessage += orderedMessage[i].getSentenceMessage()
            }
            if (fullMessage != "") {
                showAutoSentence()
            }
            else {
                hideAutoSentence()
            }

            FullSentenceContent.innerHTML = fullMessage;
        }

        function showAutoSentence()
        {
            $(ModalSenetenceContainer).removeClass("HideInactiveElement");
        }

        function hideAutoSentence()
        {
            $(ModalSenetenceContainer).addClass("HideInactiveElement");
        }

        this.getContainer = function () {
            return ModalSenetenceContainer
        }

        this.UpdateAutoSentence = updateSentence;
    }

    var AutoSentence = new sentenceCompletion();
    var AutoSentenceCOntainer= AutoSentence.getContainer()

    modalTileEvent.Dom.appendChild(ActiveContainer.Dom);
    modalTileEvent.Dom.appendChild(InActiveContainer.Dom);
    modalTileEvent.Dom.appendChild(AutoSentenceCOntainer);
    ModalActiveOptionsContainer.removeOptions = function (NewOption)
    {
        if (NewOption.Dom.parentElement != null) {
            (NewOption.Dom.parentElement.removeChild(NewOption.Dom));
        }
    }

    InActiveContainer.Options = {};
    InActiveContainer.addOptions = function (NewOption)
    {
        InActiveContainer.appendChild(NewOption.Dom);
        InActiveContainer.Show();
        InActiveContainer.Options[NewOption.DomID] = NewOption;
    }

    InActiveContainer.Show = function ()
    {
        $(InActiveContainer).removeClass("HideInactiveElement");
        $(InActiveContainer).addClass("RevealInActivePanel");
    }

    InActiveContainer.Hide = function () {
        $(InActiveContainer).addClass("HideInactiveElement");
        $(InActiveContainer).removeClass("RevealInActivePanel");
    }

    InActiveContainer.removeOptions = function (NewOption)
    {
        if (NewOption.Dom.parentElement != null) {
            (NewOption.Dom.parentElement.removeChild(NewOption.Dom));
             delete InActiveContainer.Options[NewOption.DomID]
        }

        if (Object.keys(InActiveContainer.Options).length < 1)
        {
            InActiveContainer.Hide();
        }
    }
    //Checks to see if there are any options available before showing panel
    InActiveContainer.reveal = function ()
    {
        if (Object.keys(InActiveContainer.Options).length < 1)
        {
            InActiveContainer.Hide();
            return;
        }
        InActiveContainer.Show();
    }

    InActiveContainer.unReveal = function ()
    {
        InActiveContainer.Hide();
    }

    
    var DoneContainerID = "ModalTileContainerDone";
    var DoneButton = getDomOrCreateNew(DoneContainerID);
    var Element1 = {
        LabelBefore: "I need to",
        Message:
            {
               Index: 0,
               LoopBack: function (value) {
                    var message = "";
                    if (value!="")
                    { message = "I need to " + value }

                    return message;
                }
        }
    };
    var Element2 = {
        LabelBefore: "at",
        Message: {
            Index: 1,
            LoopBack: function (value) {
                var message = "";
                if (value != "")
                {
                    message = " at " + value;
                }

                return message;
            }
        },
        DefaultText: "Location", DropDown: { url: global_refTIlerUrl + "User/Location", LookOut: "Address" }
    };
    
    var Hour = new TileInputBox({
        LabelAfter: "H", Message: {
            Index: 3,
            LoopBack: function (value) {
                var message = "";
                if (value != "") {
                    if (value > 1) {
                        message = value + " hours";
                    }
                    else
                    {
                        if (value != 0)
                        {
                            message = value + " hour";
                        }
                    }
                    
                }

                return message;
            }
        }, InputCss: { width: "1em" }
    }, undefined, undefined, Exit, undefined, null, AutoSentence)
    var Min = new TileInputBox({
        LabelAfter: "M", Message: {
            Index: 4,
            LoopBack: function (value) {
                var message = "";
                if (value != "") {
                    if (value > 1) {
                        message =" "+ value + " minutes";
                    }
                    else {
                        if (value != 0)
                        {
                            message = " " + value + " minute";
                        }
                        

                    }
                }
                return message;
            }
        }, InputCss: { width: "1em" }
    }, undefined, undefined, Exit, undefined, null, AutoSentence)
    var Day = new TileInputBox({ LabelAfter: "D", InputCss: { width: "1em" } }, undefined, undefined, Exit)
    //var AllTimeLements = [Hour, Min, Day];
    var AllTimeLements = [Hour, Min];


    var Element3 = {
        LabelBefore: "It will Take", Message: {
            Index: 2,
            LoopBack: function (value) {
                var message = "";
                if (value != "") {
                    message = ". It will Take ";
                }

                return message;
            }
        }, SubTileInputBox: AllTimeLements, HideInput: true, HideInputDomain: true
    };
    var Element4 = {
        LabelBefore: "and I need to get it done by", Message: {
            Index: 5,
            LoopBack: function (value) {
                var message = "";
                if (value != "") {

                    value = new Date(value).toDateString();;
                    message = " and I need to get it done by " + value;
                }

                return message;
            }
        }, TriggerDone: true
    };
    //debugger;
    var RepetionSlider=PopulateSliders(ModalActiveOptionsContainer, InActiveContainer);
    InActiveContainer.Hide();


    function Exit()///forces the removal of the Div
    {
        if (modalTileEvent != null)
        {
            $(modalTileEvent.Dom).empty();
            if (modalTileEvent.Dom.parentElement!=null)
            {
                (modalTileEvent.Dom.parentElement.removeChild(modalTileEvent.Dom));
            }
        }
        ActivateUserSearch.setSearchAsOn();
    }

    

    var AllTileElements = [];

    TileInputBox.DoneButton = new TileDoneButton(InActiveContainer);//creates a done button and makes it a static member of TileInputBox.
    TileInputBox.DoneButton.GetDom().onkeypress =(
        function (e) {
            if (e.which == 13) {
                SendData()
            }
        })
    AddTiledEvent.Exit = Exit;
    AllInputData.push(Element1);
    AllInputData.push(Element2);
    AllInputData.push(Element3);
    AllInputData.push(Element4);
    var LastElement = new TileInputBox(AllInputData[AllInputData.length - 1], ModalContentContainer, DoneButton, Exit, undefined, null, AutoSentence);
    AllTileElements.push(LastElement);
    
    for (var i = AllInputData.length-2, j = AllInputData.length - 1; i >= 0; i--, j--)
    {
        AllInputData[i].NextElement = LastElement;
        LastElement = new TileInputBox(AllInputData[i], ModalContentContainer, DoneButton, Exit, undefined, null, AutoSentence);
        AllTileElements.push(LastElement);
    }

    var BoundTimePicerObj = BindDatePicker(TileInputBox.Dictionary[Element4.ID].Me.getInputDom());//Set inbox as date time picker box
    

    BoundTimePicerObj.on("show", function () {
        //alert("show triggered");
        var keyEntryFunc = TileInputBox.Dictionary[Element4.ID].Me.getKeyCallBackFunc()
        var EndTimeInput = TileInputBox.Dictionary[Element4.ID].Me.getInputDom()
        //debugger;
        EndTimeInput.removeEventListener("keydown", keyEntryFunc);
    })

    BoundTimePicerObj.on("hide", function () {
        //alert("hide triggered");
        var keyEntryFunc = TileInputBox.Dictionary[Element4.ID].Me.getKeyCallBackFunc()
        var EndTimeInput = TileInputBox.Dictionary[Element4.ID].Me.getInputDom()
        EndTimeInput.addEventListener("keydown", keyEntryFunc);
        keyEntryFunc();
    })


    //handles UI change when there is a change in deadline input box data
    $(TileInputBox.Dictionary[Element4.ID].Me.getInputDom()).change(function () {
        var currValue = TileInputBox.Dictionary[Element4.ID].Me.getInputDom().value;
        currValue = currValue.split(" ")
        currValue = currValue.join("");
        if (currValue != "") {
            TileInputBox.Dictionary[Element4.ID].Me.getInputDom().focus();//ensures focus is returned after clicking date
            TileInputBox.DoneButton.Show();
        }
        else
        {
            TileInputBox.DoneButton.Hide();
        }
    })

    var FirstElement = LastElement;
    
    while (LastElement.NextElement!=undefined)
    {
        AddToTileContainer(LastElement, ModalContentContainer);
        //ModalContentContainer.Dom.appendChild(LastElement.FullContainer.Dom);
        LastElement = LastElement.NextElement;
    }
    AddToTileContainer(LastElement, ModalContentContainer);
    //ModalContentContainer.Dom.appendChild(LastElement.FullContainer.Dom);
    
    

    //sends schedule information to backend
    function SendData()
    {
        var Splits = RepetionSlider.getAllElements()[0].TileInput;
        var RepetionChoice = RepetionSlider.getAllElements()[1].TileInput;

        var SendIt = prepSendTile(Element1.TileInput.getInputDom(), Element2.TileInput.getInputDom(), Splits.getInputDom(), Hour.getInputDom(), Min.getInputDom(), Element4.TileInput.getInputDom(), RepetionChoice.getInputDom(), RepetionSlider.getStatus());
        if (TileInputBox.DoneButton.getStatus()) {
            SendIt();
            //AddTiledEvent.Exit();
        }
        else
        {
            alert("please provide viable deadline");
        }
    }



    function UIAddTileUITrigger(e)
    {
        if (e.which == 27)//escape key press
        {
            document.removeEventListener("keydown", UIAddTileUITrigger);
            AddTiledEvent.Exit()
        }
        
    }
    
    
    TileInputBox.Send = SendData;
    
    $(TileInputBox.DoneButton.GetDom()).click(SendData);
    ModalDoneContentContainer.Dom.appendChild(TileInputBox.DoneButton.GetDom());

    InvisiblePanel.Dom.appendChild(modalTileEvent.Dom);
    document.addEventListener("keydown", UIAddTileUITrigger);
    //debugger;
    FirstElement.reveal();
    FirstElement.forceFocus();
    return ModalContentContainer;
}


//Creates the Tile Done Button
function TileDoneButton(InActivePanel)
{
    var AddTileButtonDoneButton = "TileInputDoneButton";
    var DoneButton = getDomOrCreateNew(AddTileButtonDoneButton);
    var ReturnText = getDomOrCreateNew("ReturnText", "span");
    ReturnText.Dom.innerHTML = "Return ";
    var OrText = getDomOrCreateNew("Or", "span");
    OrText.Dom.innerHTML = " or ";
    var ClickHereText = getDomOrCreateNew("ClickHereText", "span");
    ClickHereText.Dom.innerHTML = "Click Here ";
    var WhenDoneText = getDomOrCreateNew("WhenDone", "span");
    WhenDoneText.Dom.innerHTML = " When Done.";
    DoneButton.Dom.appendChild(ReturnText.Dom);
    DoneButton.Dom.appendChild(OrText.Dom);
    DoneButton.Dom.appendChild(ClickHereText.Dom);
    DoneButton.Dom.appendChild(WhenDoneText.Dom);
    $(DoneButton.Dom).click(SendDataToBackEnd);
    var ready = false;
    this.Show = function ()
    {
        $(DoneButton.Dom).removeClass("HideTileDoneButton");
        InActivePanel.reveal();
        ready = true;
    }

    this.Hide = function ()
    {
        $(DoneButton.Dom).addClass("HideTileDoneButton");
        InActivePanel.Hide();
        ready = false;
    }

    this.GetDom = function ()
    {
        return DoneButton.Dom;
    }
    this.Hide();

    function onFocus()
    {

    }
    $(DoneButton.Dom).attr('tabindex', 0).focus(onFocus);


    function RevealSecondPanel()
    {
        InActivePanel.reveal();
    }

    this.RevealSecondPanel = RevealSecondPanel;
    this.getStatus = function ()
    {
        return ready;
    }
    
}

function SendDataToBackEnd()
{

}



function TileInputBox(TabElement, ModalContainer, SendTile, Exit, HideInput, getDataFunction, SenetenceCompletion)
{
    var LabelBefore = TabElement.LabelBefore == null ? "" : TabElement.LabelBefore;
    var LabelAfter = TabElement.LabelAfter == null ? "" : TabElement.LabelAfter;
    var myTabElement = TabElement;
    
    var InputBoxLabelBeforeID = "InputBoxLabelBefore" + ++TileInputBox.ID;
    var InputBoxLabelAfterID = "InputBoxLabelAfter" + ++TileInputBox.ID;
    var MyID = "TileInputID" + TileInputBox.ID;
    TabElement.ID = MyID;
    var meTHis = this;
    TileInputBox.Dictionary[MyID] = { TabElement: myTabElement, Me: meTHis };
    TabElement.TileInput = meTHis;
    var InputBoxLabelBefore = getDomOrCreateNew(InputBoxLabelBeforeID, "label");
    InputBoxLabelBefore.Dom.innerHTML = LabelBefore;
    var InputBoxLabelAfter = getDomOrCreateNew(InputBoxLabelAfterID, "label");
    InputBoxLabelAfter.Dom.innerHTML = LabelAfter;

    if (TabElement.LabelCSS!=undefined)
        $(InputBoxLabelBefore.Dom).css(TabElement.LabelCSS);

    TabElement.isInFocus = false;
    var NextElement = { Data: TabElement.NextElement }
    var InputBoxID = "InputBox" + TileInputBox.ID++;
    this.NextElement = NextElement.Data;
    NextElement.Previous = this;
    var InputBox = getDomOrCreateNew(InputBoxID, "input");
    var InputDataDomain = InputBox;
    InputDataDomain.CleanUp = function ()
    {
        return;
    }
    
    var labelAndInputContainerID = "labelAndInputContainer" + TileInputBox.ID;;
    var labelAndInputContainer = getDomOrCreateNew(labelAndInputContainerID);

    
    var invisibleSpan = getDomOrCreateNew("measureSpan" + TileInputBox.ID, "span");
    $(invisibleSpan.Dom).addClass("invisibleSpan");
    //labelAndInputContainer.Dom.appendChild(invisibleSpan.Dom);
    $(InputBox.Dom).addClass("TileInput");

    //$(labelAndInputContainer.Dom).addClass("NonReveal");
    //$(labelAndInputContainer.Dom).addClass("labelAndInputContainer");
    
    var OtherElements = [];

    GenerateAutoSuggest();
    GenerateAlreadyCreatedBoxes();
    DeployInputSettings();

    //fuction generates and binds all elements for a drop down menu option
    function GenerateAutoSuggest()
    {
        var dropDown;
        var JSONProperty;

        function LoopBack(Data,Container)
        {
            $(Container).removeClass("NonReveal");
            CleanUp();
            if (!TabElement.isInFocus)
            {
                return;
            }
            TabElement.DropDown.AllDoms = [];
            SetContainerToBottomOfInput();
            function generateEachDom(eachData)
            {
                var DropDownElement = getDomOrCreateNew(generateEachDom.ID++);
                DropDownElement.Dom.innerHTML = eachData[JSONProperty];
                Container.Dom.appendChild(DropDownElement.Dom);
                TabElement.DropDown.AllDoms.push(DropDownElement.Dom);
                SelectDropOption(DropDownElement.Dom)
                TabElement.DropDown.status = true;
            }

            function SetContainerToBottomOfInput()
            {
                var Position = $(InputBox.Dom).position();
                var Left = 50;// Position.left;
                var Top = Position.top;
                var height = $(InputBox.Dom).height();
                Top += height;
                $(TabElement.DropDown.AutoSuggestContainer).css({ left: Left + "px", top: Top + "px", position: "absolute", width: "calc(100% - 100px)" });
            }

            function SelectDropOption(Dom)
            {
                function SetAsActive()
                {
                    
                    var mInputBox = dropDown.getInputBox();
                    mInputBox.value = Dom.innerHTML;
                    var FullWidth = resizeInput();
                    CleanUp();
                }

                function Onfocus()
                {
                    $(Dom).addClass("OnFocusDropDownElement");
                }

                function Outfocus()
                {
                    $(Dom).removeClass("OnFocusDropDownElement");
                }
                $(Dom).click(SetAsActive);
                Dom.Onfocus = Onfocus;
                Dom.SetAsActive = SetAsActive
                Dom.Outfocus = Outfocus;
            }

            function CleanUp()
            {
                TabElement.DropDown.CleanUp();
            }
            
            //function MonitorNavigation()
            
            var myInput = dropDown.getInputBox();
            TabElement.DropDown.Index = 0;
            TabElement.DropDown.CurrentOnFocus = null;
            TabElement.DropDown.OnUpKey = function ()
            {
                if (TabElement.DropDown.CurrentOnFocus != null)
                {
                    TabElement.DropDown.CurrentOnFocus.Outfocus();
                }
                TabElement.DropDown.AllDoms[TabElement.DropDown.Index].Onfocus();
                TabElement.DropDown.CurrentOnFocus = TabElement.DropDown.AllDoms[TabElement.DropDown.Index];
                --TabElement.DropDown.Index;
                TabElement.DropDown.Index = (TabElement.DropDown.AllDoms.length + TabElement.DropDown.Index) % TabElement.DropDown.AllDoms.length;
                $(myInput).focus();
            }

            TabElement.DropDown.OnDownKey = function ()
            {
                if (TabElement.DropDown.CurrentOnFocus != null) {
                    TabElement.DropDown.CurrentOnFocus.Outfocus();
                }
                if (TabElement.DropDown.AllDoms.length < 0)
                {
                    return;
                }
                TabElement.DropDown.AllDoms[TabElement.DropDown.Index].Onfocus();
                TabElement.DropDown.CurrentOnFocus = TabElement.DropDown.AllDoms[TabElement.DropDown.Index];
                ++TabElement.DropDown.Index;
                TabElement.DropDown.Index = (TabElement.DropDown.AllDoms.length + TabElement.DropDown.Index) % TabElement.DropDown.AllDoms.length;
                $(myInput).focus();
            }
            
            
            generateEachDom.ID = 0;
            setTimeout(function () { Data.forEach(generateEachDom); },100);//Just waits for the creation of generateEachDom
            
        }

        if (TabElement.DropDown != undefined)
        {
            dropDown = new AutoSuggestControl(TabElement.DropDown.url, "GET", LoopBack, InputDataDomain.Dom);
            
            InputDataDomain = {};
            JSONProperty = TabElement.DropDown.LookOut; //   var Element3 = { Label: "Element3", DropDown: { url: global_refTIlerUrl + "CalendarEvent/Name", LookOut:  } };
            TabElement.DropDown.AutoSuggestContainer = dropDown.getAutoSuggestControlContainer();
            InputDataDomain.Dom = TabElement.DropDown.AutoSuggestContainer;
            $(TabElement.DropDown.AutoSuggestContainer).css({ position : "absolute" });
            TabElement.DropDown.status = false;
            TabElement.DropDown.CleanUp = function () {
                var SuggestedValueContainer = dropDown.getSuggestedValueContainer();
                $(SuggestedValueContainer).empty();
                TabElement.DropDown.status = false;
            }


            InputDataDomain.CleanUp = function ()
            {
                TabElement.DropDown.CleanUp();
            }
            InputBox.Dom = dropDown.getInputBox();
            $(dropDown.getInputBox()).focus(onFocus)
            $(dropDown.getInputBox()).focusout(outFocus);
        }
    }

    function GenerateAlreadyCreatedBoxes()
    {
        if (TabElement.SubTileInputBox != undefined)
        {
            TabElement.SubTileInputBox.forEach(revealEachElement);
        }

        function revealEachElement(eachSubTileInputBox)
        {
            //debugger;
            OtherElements= OtherElements.concat(eachSubTileInputBox.getAllElements());
            eachSubTileInputBox.ReplaceNextElement(NextElement.Data);
            eachSubTileInputBox.reveal();
        }
    }

    function DeployInputSettings()
    {
        if (TabElement.InputCss!=undefined)
        {
            $(InputBox.Dom).css(TabElement.InputCss);
        }
        
    }


    var tabfunction = function ()
    {
        if (NextElement.Data!= undefined)
        {

            NextElement.Data.reveal();
        }
    };
    var reveal = function ()
    {
        var AllElements = getAllElements();
        for (var i = 0; i < AllElements.length; i++) {
            $(AllElements[i]).addClass("reveal");
            $(AllElements[i]).removeClass("NonReveal");
        }
    }

    this.reveal = reveal;

    var unReveal = function ()
    {
        var AllElements = getAllElements();
        for (var i = 0; i < AllElements.length; i++) {
            $(AllElements[i]).addClass("NonReveal");
            //$(AllElements[i]).removeClass("NonReveal");
        }
    }

    this.forceFocus = function () {
        $(InputBox.Dom).focus();
        InputBox.Dom.setAttribute("autofocus", true);
        
    }

    this.getInputDom=function ()
    {
        return InputBox.Dom;
    }

    this.getLabelBefore=function()
    {
        return InputBoxLabelBefore.Dom;
    }

    this.getLabelAfter = function () {
        return InputBoxLabelAfter.Dom;
    }
    
    //Function tries to attached to sentence completion if it has a message.
    function getSentenceCompletionMessage()
    {
        var retValue = function () { return ""};
        if (TabElement.Message != null)
        {
            retValue = function ()
            {
                var loopBackArg ="";
                if (TabElement.SubTileInputBox != undefined) {
                    for(var i=0;i< TabElement.SubTileInputBox.length;i++)
                    {
                        loopBackArg+=TabElement.SubTileInputBox[i].getSentenceCompletionMessage();
                    }
                }
                if (InputBox != null)//scenario where tileinputbox has no input box, usually when label is used
                {
                    loopBackArg += InputBox.value;
                }
                return TabElement.Message.LoopBack(loopBackArg);
            }

            TabElement.getSentenceMessage = retValue;
            SenetenceCompletion.addTileInput(TabElement);
        }

        return retValue;
    }

    
    var getAllElements=function()
    {
        var retValue = [InputBoxLabelBefore, InputBox, InputBoxLabelAfter,InputDataDomain.Dom, invisibleSpan];
        for (var i = 0; i < OtherElements.length; i++)
        {
            retValue.push(OtherElements[i]);
        }
        return retValue;
    }

    this.getAllElements = getAllElements;

    this.getID = function ()
    {
        return MyID;
    }

    var resizeInput = function() {
        var value = $(InputBox.Dom).val();
        var span = invisibleSpan.Dom;
        span.innerHTML = value;
        var span_width = $(span).width() + 20;
        $(InputBox.Dom).width(span_width);
        return span_width;
    }

    var ResizeInputTrim = function ()
    {
        var value = $(InputBox.Dom).val();
        var span = invisibleSpan.Dom;
        span.innerHTML = value;
        var span_width = $(span).width() + 8;
        $(InputBox.Dom).width(span_width);
        return span_width;
    }

    function KeyEntry(e)
    {
        resizeInput();
        setTimeout(function ()//delaying just to allow for input to post to UI on keydown
        {
            SenetenceCompletion.UpdateAutoSentence();
        },10)
        
        if (e == null)//handles scenario when this called randomly. E.g when triggered by calendar UI trigger
        {
            return;
        }
        e.stopPropagation();
        
        if ((e.shiftKey) && (e.which == 9))
        {
            return;
        }
        
        

        if (e.which == 9)
        {
            if (myTabElement.TriggerDone == true)//checks if the tab should navigate to done
            {

            }
        }

        if (e.which == 13)
        {
            if (TabElement.DropDown != undefined)
            {
                if(TabElement.DropDown.status)
                {
                    TabElement.DropDown.CurrentOnFocus.SetAsActive();
                    return;
                }
            }
            TileInputBox.Send();

        }
        if (e.which == 27)//escape key press
        {
            AddTiledEvent.Exit()
        }

        if ((e.which == 38))
        {
            if (TabElement.DropDown != undefined)
            {
                TabElement.DropDown.OnUpKey()
            }
        }

        if (((e.which == 40)))
        {
            if (TabElement.DropDown != undefined)
            {
                if (TabElement.DropDown.OnDownKey!=undefined)
                {
                    TabElement.DropDown.OnDownKey();
                }
            }
        }
        
    }

    function CleanUp()// cleans up the UI element when it goes out of focus
    {

    }
    
    this.ReplaceNextElement=function(newNext)
    {
        NextElement.Data = newNext;
    }

    function onFocus()//triigers reveal of next element when the tab button is pressed
    {
        $(InputBox.Dom).addClass("FocusTileEvent");
        tabfunction();
        InputBox.Dom.addEventListener("keydown", KeyEntry);
        TabElement.isInFocus = true;
        if (myTabElement.TriggerDone == true)//checks if the tab should navigate to done
        {
            //TileInputBox.DoneButton.Show();
        }
    }

    function outFocus()
    {
        $(InputBox.Dom).removeClass("FocusTileEvent");
        TabElement.isInFocus = false;
        setTimeout(function () { ResizeInputTrim(); }, 0);
    }

    $(InputBox.Dom).focusout(InputDataDomain.CleanUp);//handles clean up when element goest out of focus

    this.getFocusStatus= function()
    {
        return TabElement.isInFocus;
    }

    this.getKeyCallBackFunc = function ()//function returns the KeyEntry function.
    {
        return KeyEntry;
    }


    
    var AllElements = getAllElements();
    for (var i = 0; i < AllElements.length; i++)
    {
        $(AllElements[i]).addClass("tileInputBoxElement");
    }


    //labelAndInputContainer.Dom.appendChild(InputBoxLabelBefore.Dom);
    //labelAndInputContainer.Dom.appendChild(InputBox.Dom);
    //labelAndInputContainer.Dom.appendChild(InputBoxLabelAfter.Dom);

    
    //labelAndInputContainer.Dom.appendChild(InputDataDomain.Dom);
    /*
    function InsertEachElement(EachSubTile)
    {
        labelAndInputContainer.Dom.appendChild(EachSubTile.FullContainer.Dom);
    }
    if (TabElement.SubTileInputBox != undefined)
    {
        (labelAndInputContainer.Dom.removeChild(InputBox.Dom));
        
        TabElement.SubTileInputBox.forEach(InsertEachElement);
    }
    */
    unReveal();
    if (TabElement.DefaultText!=null)
    {
        InputBox.Dom.setAttribute("placeholder", TabElement.DefaultText);
    }
    $(InputBox.Dom).focus(onFocus)
    $(InputBox.Dom).focusout(outFocus);

    this.getSentenceCompletionMessage = getSentenceCompletionMessage();

    if (TabElement.HideInput)
    {
        InputBox = null;
    }

    if (TabElement.HideInputDomain)
    {
        InputDataDomain.Dom = null;
    }
    
    //this.FullContainer = labelAndInputContainer;
}

TileInputBox.ID = 0;
TileInputBox.Dictionary = {};
TileInputBox.DoneButton = {}





function generateEndContainer()
{
    var EndDateTimeContainer = getDomOrCreateNew("EndTimeContainer");
    var EndDateInputContainer = getDomOrCreateNew("EndDateInputContainer");
    var EndDateInput = getDomOrCreateNew("EndDateInput", "Input");
    EndDateInput.Dom.setAttribute("placeholder", "Deadline Date");
    var EndTimeInputContainer = getDomOrCreateNew("EndTimeInputContainer");
    var EndTimeInput = getDomOrCreateNew("EndTimeInput", "Input");
    EndTimeInput.Dom.setAttribute("placeholder", "Deadline Time");

    EndTimeInputContainer.Dom.appendChild(EndTimeInput.Dom);
    EndDateInputContainer.Dom.appendChild(EndDateInput.Dom);
    BindDatePicker(EndDateInput.Dom);
    BindTimePicker(EndTimeInput.Dom);
    EndDateTimeContainer.getDateTimeData = getFullTimeFromEntry(EndTimeInput, EndDateInput, OneDayInMs);

    EndDateTimeContainer.Dom.appendChild(EndTimeInputContainer.Dom);
    EndDateTimeContainer.Dom.appendChild(EndDateInputContainer.Dom);
    EndDateTimeContainer.Selector = { Container: EndDateTimeContainer.Dom, Time: EndTimeInputContainer, Date: EndDateInputContainer };
    return EndDateTimeContainer;
}





function generateRepetitionContainer()
{
    var RepetitionContainer = getDomOrCreateNew("RepetitionContainer");
    var FrequencySelectorContainer = getDomOrCreateNew("FrequencySelectorContainer");
    var RepetitionEndDateTimeContainer = getDomOrCreateNew("RepetitionEndTimeContainer");
    var RepetitionEndTimeInput = getDomOrCreateNew("RepetitionEndTimeInputContainer");
    var RepetitionEndTimeInputContainer = getDomOrCreateNew("RepetitionEndTimeInput", "Input");
    var RepetitionEndDateInput = getDomOrCreateNew("RepetitionEndDateInputContainer");
    var RepetitionEndDateInputContainer = getDomOrCreateNew("RepetitionEndDateInput", "Input");
}





function splitInputText()
{
    var splitInputContainer = getDomOrCreateNew("splitInputContainer");
    var splitInput = getDomOrCreateNew("splitInput", "input");
    splitInput.Dom.setAttribute("placeholder", "Counts(Default 1)");
    //splitInput.Dom.setAttribute("value",1);
    splitInputContainer.Dom.appendChild(splitInput.Dom);
    splitInputContainer.Selector = { Container: splitInputContainer.Dom,Input:splitInput };
    return splitInputContainer;
}



function generateSubmitButton()
{
    var SubmitButtonContainer = getDomOrCreateNew("SubmitButtonContainer");
    var SubmitButton = getDomOrCreateNew("SubmitButton", "Button");
    SubmitButton.Dom.innerHTML = "Add New Event";
    SubmitButtonContainer.Dom.appendChild(SubmitButton.Dom);
    SubmitButtonContainer.Selector = { Container: SubmitButtonContainer.Dom, Button: SubmitButton };
    return SubmitButtonContainer;
}


function BindSubmitClick(Name, Address, AddressNick, Splits, Start, End, EventNonRigidDurationHolder, RepetitionEnd, RigidFlag, CalendarColor,CloseEventAddition)
{
    var EventLocation = new Location(AddressNick, Address);
    var EventName = Name;
    if (Splits == "")
    {
        Splits = 1;
    }
    var EventDuration = EventNonRigidDurationHolder.Selector.TimeHolder();
     //CalendarColor = { r: 200, g: 200, b: 200,a:1,selection:0 };
    CalendarColor = { r: CalendarColor.r, g: CalendarColor.g, b: CalendarColor.b, s: CalendarColor.Selection, o: CalendarColor.a };

    var EventStart = Start.getDateTimeData();
    var EventEnd = End.getDateTimeData();
    var DurationInMS = (parseInt(EventDuration.Days) * OneDayInMs) + (parseInt(EventDuration.Hours) * OneHourInMs) + (parseInt(EventDuration.Mins) * OneMinInMs)
    if (DurationInMS == 0) {
        alert("Oops please provide a duration for \"" + EventName + "\"");
        return null;
    }


    if (RigidFlag) {
        var TempEndDate = new Date(getFullTimeFromEntrytoJSDateobj(EventStart).getTime() + DurationInMS);
        EventEnd.Date = new Date(TempEndDate.getFullYear(), TempEndDate.getMonth(), TempEndDate.getDate());
        EventEnd.Time = { Hour: TempEndDate.getHours(), Minute: TempEndDate.getMinutes() };
    }

    var repeteOpitonSelect = "none";
    var DayPlusOne = new Date(CurrentTheme.Now);
    var Day = DayPlusOne.getDate();
    var Month = DayPlusOne.getMonth() + 1;
    var Year = DayPlusOne.getFullYear();
    var DatePickerValue = Month + "/" + Day + "/" + Year;

    var RepetitionStart = DatePickerValue;
    var RepetitionEnd = RepetitionStart;

    var NewEvent = new CalEventData(EventName, EventLocation, Splits, CalendarColor, EventDuration, EventStart, EventEnd, repeteOpitonSelect, RepetitionStart, RepetitionEnd, RigidFlag);
    NewEvent.RepeatData = null;
    if (NewEvent == null) {
        return;
    }
    NewEvent.UserName = UserCredentials.UserName
    NewEvent.UserID = UserCredentials.ID;

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
            //var myContainer = (CurrentTheme.getCurrentContainer());
            //CurrentTheme.TransitionOldContainer();
            //$(myContainer).empty();
            //myContainer.outerHTML = "";
        },
        error: function (err) {
            //var myError = err;
            //var step = "err";
            var NewMessage = "Oh No!!! Tiler is having issues modifying your schedule. Please try again Later :(";
            var ExitAfter = { ExitNow: true, Delay: 1000 };
            HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, function () { });
        }

    }).done(function (data) {
        HandleNEwPage.Hide();
        getRefreshedData();
        CloseEventAddition();
    });
}
