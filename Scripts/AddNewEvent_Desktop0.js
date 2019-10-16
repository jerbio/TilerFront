"use strict";

function addNewEvent(x, y, height, refStart)
{
    
    var AddEventPanel = getDomOrCreateNew("AddEventPanel");
    generateAddEventContainer(x, y, height, AddEventPanel.Dom, refStart);
    
}

function generatePostBackDataForTimeRestriction(RestrictionSlider)
{
    
    var RestrictionStatusButtonStatus = RestrictionSlider.getStatus();
    var RestrictionStart = RestrictionSlider.getStart();
    var RestrictionEnd = RestrictionSlider.getEnd();
    var RestrictionWorkWeek = RestrictionSlider.isWorkWeek();
    var RestrictEveryday= RestrictionSlider.isEveryDay();
    var RestrictiveWeek = RestrictionSlider.getRestrictiveWeek().getPostData();
    var RetValue = { isRestriction: RestrictionStatusButtonStatus, Start: RestrictionStart, End: RestrictionEnd, isEveryDay: RestrictEveryday, isWorkWeek: RestrictionWorkWeek, RestrictiveWeek: RestrictiveWeek }
    return RetValue;
}

function generateOfficeHours(Place)
{
    var RetValue = { WeekDayData: [], IsTwentyFourHours: false,NoWeekData:true };

    if (Place.opening_hours.weekday_text.length)
    {
        for (var i = 0; i < Place.opening_hours.weekday_text.length; i++)
        {
            var CurrentWeekday = Place.opening_hours.weekday_text[i];
            var RestrivtiveDay = createRestrictedTimeData(CurrentWeekday);
            if(RestrivtiveDay.IsTwentyFourHours)
            {
                RetValue.IsTwentyFourHours = RestrivtiveDay.IsTwentyFourHours;
                break;
            }
            RetValue.NoWeekData = false;
            RetValue.WeekDayData.push(RestrivtiveDay);
        }
    }

    function createRestrictedTimeData(WeekdayText)
    {
        var WeekData = WeekdayText.split(":");
        var DayData = WeekData [0].trim();
        var DayIndex = WeekDays.indexOf(DayData);
        var TimeData = WeekData .slice(1,WeekData .length).join(":").trim();
        var TimeSection =TimeData .split(",");

        var RetValue = {DayIndex:DayIndex, Start:null, End:null,IsTwentyFourHours:false};

        function PickTimeFrames(DayOfWeek,TimeSections)
        {
            var DayIndex = WeekDays.indexOf(DayOfWeek);
            //var RetValue = { DayIndex: DayIndex, Start: null, End: null, IsTwentyFourHours: false, IsClosed:null };

            var AllTimeDataStart = [];
            var AllTimeDataEnd = [];

            for (var i = 0 ; i < TimeSections.length; i++)
            {
                var TimeText = TimeSections[i].toLowerCase();
                var timeee = "hakh hkjahjks";
                
                if(TimeText !="open 24 hours")
                {
                    if(TimeText !="closed")
                    {
                        var BeginAndEndArray = TimeText.split("–");
                        var Begin = BeginAndEndArray[0].trim();
                        Begin = Begin.trim();
                        //Begin = Begin.slice(0, -1);
                        Begin =spliceSlice(Begin, Begin.length - 3, 0, " ");
                        var End = BeginAndEndArray[1].trim();
                        End = spliceSlice(End, End.length - 3, 0, " ");
                        var Begin = Date.parse((new Date()).toLocaleDateString()+" " + Begin);
                        End = Date.parse((new Date()).toLocaleDateString() + " " + End);
                        var EndData = new Date(End);
                        if ((EndData.getHours() == 0) && (EndData.getMinutes() == 0))
                        {
                            End -= 1000;
                        }
                        AllTimeDataStart.push(Begin);
                        AllTimeDataEnd.push(End);
                        RetValue.IsClosed = false;
                    }
                    else
                    {
                        RetValue.IsClosed = true;
                    }
                }
                else
                {
                    RetValue.IsTwentyFourHours = true;
                    return RetValue;
                    break;
                }
            }
            AllTimeDataStart.sort(function (a, b) { return (a) - (b) });
            AllTimeDataEnd.sort(function (a, b) { return (a) - (b) });

            for (var i = 0 ; i < AllTimeDataStart.length; i++)
            {
                AllTimeDataStart[i] = new Date(AllTimeDataStart[i]);
            }


            for (var i = 0; i < AllTimeDataEnd.length; i++)
            {
                AllTimeDataEnd[i] = new Date(AllTimeDataEnd[i]);
            }

            RetValue.Start = AllTimeDataStart[0];
            RetValue.End = AllTimeDataEnd[AllTimeDataEnd.length - 1];
            return RetValue;
        }

        var RetValue = PickTimeFrames(DayData, TimeSection);


        return RetValue;
        /*
        0: "Monday: Open 24 hours"
        1: "Tuesday: Open 24 hours"
        2: "Wednesday: Open 24 hours"
        3: "Thursday: Open 24 hours"
        4: "Friday: Open 24 hours"
        5: "Saturday: Open 24 hours"
        6: "Sunday: Open 24 hours"
        */
    }

    return RetValue;
}




function spliceSlice(str, index, count, add) {
    return str.slice(0, index) + (add || "") + str.slice(index + count);
}


function prepSendTile(NameInput, AddressInput, NickNameSlider, SpliInput, HourInput, MinuteInput, DeadlineInput, RepetitionInput, RepetitionFlag, ColorSelection,TimeRestrictions)
{
    return function ()
    {
        var calendarColor = ColorSelection;
        var restrictionData = generatePostBackDataForTimeRestriction(TimeRestrictions);
        var NickName = "";
        if (NickNameSlider.getStatus())
        {
            NickName = NickNameSlider.getAllElements()[0].TileInput.getInputDom().value;
        }
        var newEvent= SubmitTile(NameInput.value, AddressInput,NickName, SpliInput.value, HourInput.value, MinuteInput.value, DeadlineInput.value, RepetitionInput.value, calendarColor, RepetitionFlag, restrictionData, AddressInput);
        SendScheduleInformation(newEvent, global_ExitManager.triggerLastExitAndPop);
    }
}

function SubmitTile(Name, AddressInput,AddressNick, Splits, Hour, Minutes, Deadline, Repetition, CalendarColor,RepetitionFlag,TimeRestrictions)
{
    var DictOfData = {};
    DictOfData["day"] = { Range: OneDayInMs, Type: { Name: "Daily", Index: 0 }, Misc: null }
    DictOfData["week"] = { Range: OneWeekInMs, Type: { Name: "Weekly", Index: 1 }, Misc: { AllDoms: [] } }
    DictOfData["month"] = { Range: FourWeeksInMs, Type: { Name: "Monthly", Index: 2 }, Misc: null }
    DictOfData["year"] = { Range: OneYearInMs, Type: { Name: "Yearly", Index: 3 }, Misc: null }


    var EventName = Name;
    /*
    if (!EventName)
    {
        alert("Oops your tile needs a name");
        return null;
    }
    */
    let Address = AddressInput.value
    var LocationAddress = Address;
    let LocationIsVerified = AddressInput.LocationIsVerified;
    var LocationNickName = AddressNick;
    var EventLocation = new Location(LocationNickName, LocationAddress, LocationIsVerified, AddressInput.LocationId);
    Hour = Hour != "" ? Hour : 0;
    Minutes = Minutes != "" ? Minutes : 0;

    
    var Start = new Date();
    var EventStart = {}
    EventStart.Date = new Date(Start.getFullYear(), Start.getMonth(), Start.getDate());
    EventStart.Time = { Hour: 0, Minute: 0 };
    var End = new Date(Deadline);
    CalendarColor = { r: CalendarColor.r, g: CalendarColor.g, b: CalendarColor.b, s: CalendarColor.Selection, o: CalendarColor.a };

    var EventDuration = { Days: 0, Hours: Hour, Mins: Minutes };

    var DurationInMS = (parseInt(EventDuration.Days) * OneDayInMs) + (parseInt(EventDuration.Hours) * OneHourInMs) + (parseInt(EventDuration.Mins) * OneMinInMs)
    /*
    if (DurationInMS == 0) {
        alert("Oops please provide a duration for \"" + EventName + "\"");
        return null;
    }
    */

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
            /*
            if (repeteOpitonSelect.Range > FullRange)//checks if the given deadline extends past the range for a selected repetition sequence. e.g If user selects weekly, this line checks if range is between start and end is larger than 7 days
            {
                alert("please check your repetition, you dont have up to a " + Repetition + " before deadline");
                return;
            }
            */

            //End = new Date(Start.getTime() + repeteOpitonSelect.Range);
        }
        else
        {
            alert("Seems like you have invalid data for repetition. Please check your repetition");
            return;
        }
        
    }
    else
    {
        if (!RepetitionFlag)
        {
            Splits = 1;
        }
    }
    var EventEnd = {}
    EventEnd.Date = new Date(End.getFullYear(), End.getMonth(), End.getDate());
    EventEnd.Time = { Hour: 23, Minute: 59 };
    
    var NewEvent = new CalEventData(EventName, EventLocation, Splits, CalendarColor, EventDuration, EventStart, EventEnd, repeteOpitonSelect, RepetitionStart, RepetitionEnd, false,TimeRestrictions);
    //NewEvent.RepeatData = null;
    if (NewEvent == null) {
        return;
    }

    return NewEvent;
}

/*generates modal "Add New Event & Add New Tile" for creating new item. Note: width is distance in pixels between left click and End of window */
function generateModal(x, y, height, width,WeekStart, RenderPlane,UseCurrentTime)
{
    //return;
    
    initializeUserLocation();

    if (generateModal.isOn)
    {
        global_ExitManager.triggerLastExitAndPop();
        generateModal.isOn = false;
        return;
    }

    
    global_ExitManager.triggerLastExitAndPop();
    generateModal.isOn = true;
    var modalAddDom = getDomOrCreateNew("AddModalDom");
    $(modalAddDom).addClass("setAsDisplayNone");//adding this so that motion unnecesary reposition isnt noticed. It'll get removed at the end of func
    var weekDayWidth = $($(".DayContainer")[0]).width();
    var AddTile = getDomOrCreateNew("AddTileDom", "button");
    var AddEvent = getDomOrCreateNew("AddEventDom", "button");
    var SpanEscape = getDomOrCreateNew("SpanEscape", "span");
    SpanEscape.Dom.innerHTML=("Press Escape to Exit.");
    AddEvent.Dom.innerHTML=("New Event");
    AddTile.Dom.innerHTML=("New Tile");
    modalAddDom.Dom.appendChild(AddTile.Dom);
    modalAddDom.Dom.appendChild(AddEvent.Dom);
    modalAddDom.Dom.appendChild(SpanEscape.Dom);
    $(AddTile.Dom).addClass("SubmitButton");
    $(AddEvent.Dom).addClass("SubmitButton");
    RenderPlane.appendChild(modalAddDom.Dom);
    var modalHeight = ($(modalAddDom).height());
    var modalWidth= ($(modalAddDom).width());
    var MaxY = height -modalHeight;
    var MaxX = width -modalWidth;
    var modalXPos = x > MaxX?(x-modalWidth):x;
    var modalYPos = y > MaxY ?(y-modalHeight):y;
    if (height<10)// hack to ensure selection of add button
    {
        modalYPos = 0;
    }
    

    modalAddDom.Dom.style.left = modalXPos + "px";
    modalAddDom.Dom.style.top = modalYPos + "px";
    $(AddEvent.Dom).click(function () {
        (modalAddDom.Dom.parentElement.removeChild(modalAddDom.Dom));
        var floatalTime = 0;
        var Hour = 0
        var Min = 0;
        var WeekDayIndex = 0;
        var myDate = new Date(new Date().getTime() + OneHourInMs);
        myDate.setMinutes(0);
        var NewDay = 0;
        if(!UseCurrentTime)
        {
            floatalTime = (y ) / (height );// Hack alert the sutractions are hacks to make it work within the UIrenderplace.
            Hour = Math.floor((floatalTime) * 24);
            Min = 0;
            WeekDayIndex = Math.floor(x / weekDayWidth);
            myDate = new Date(WeekStart);
            NewDay = myDate.getDate() + WeekDayIndex
            myDate.setDate(NewDay);
            myDate.setHours(Hour);
            myDate.setMinutes(0);
        }
        

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

    /*function removePanel(e) {
        if (e.which == 27) {
            $(document).off("keydown", document, removePanel);
            CloseModal();
        }
        e.stopPropagation();
    }*/

    function removePanel()
    {
        
        CloseModal();
    }

    global_ExitManager.addNewExit(removePanel);
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
        ///*
        setTimeout(function () {
            if (!isDescendant(modalAddDom, document.activeElement)) {
                //$(document).off("keydown", document, removePanel);
                global_ExitManager.triggerLastExitAndPop();
            }
        }, 1);
        //*/

        

        
    };
    $(modalAddDom).removeClass("setAsDisplayNone");
    
    
    $(modalAddDom.Dom).attr('tabindex', 0).focus();
    AddCloseButoon(modalAddDom,false);

}
generateModal.isOn = false;

function CloseModal()
{
    setTimeout(function () { generateModal.isOn = false; }, 200);
    
    var myAddPanel = getDomOrCreateNew("AddModalDom");
    if (myAddPanel.Dom.parentElement != null)
    {
        myAddPanel.Dom.parentElement.removeChild(myAddPanel.Dom);
        //generateModal.isOn = false;
    }

}

function generatePeek(CalEvent,Container)
{
    //var CalEvent = new CalEventData();
    var CalEndTime =null;
    var TotalDuration=null;
    var peekValidityTest = isCalEvenValidForPeek(CalEvent)
    if (peekValidityTest.isError)
    {
        //Container.innerHTML = "not peekable because " + peekValidityTest.ErrorMessage;
        HidePeekUI(Container)
        return;
    }

    
    createPeekUI(CalEvent, Container)
    //RevealPeekUI(Container, PeekData);

    return;
    /*
    if ((TotalDuration != null) && (CalEndTime != null))
    {
        createPeekUI(CalEvent, Container)
    }
    */

    function createPeekUI(CalEvent, Container)
    {
        if (createPeekUI.Connection!=null)
        {
            createPeekUI.Connection.abort();
            createPeekUI.Connection = null;
        }

        CalEvent.UserName = UserCredentials.UserName
        CalEvent.UserID = UserCredentials.ID;
        var TimeZone = new Date().getTimezoneOffset();
        CalEvent.TimeZoneOffset = TimeZone;

        var url = global_refTIlerUrl + "Schedule/Peek";
        preSendRequestWithLocation(CalEvent);
        getRefreshedData.disableDataRefresh();
        createPeekUI.Connection = $.ajax({
            type: "POST",
            url: url,
            data: CalEvent,
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
                var b = 3;
                RevealPeekUI(Container, response.Content);
                //getRefreshedData.enableDataRefresh(true);
                //affirmNewEvent(response);
                createPeekUI.Connection = null;

            },
            error: function (err) {
                //var myError = err;
                //var step = "err";
                //var NewMessage = "Oh No!!! Tiler is having issues modifying your schedule. Please try again Later :(";
                //var ExitAfter = { ExitNow: true, Delay: 1000 };
            }

        }).done(function (response)
        {
        });
    }

    createPeekUI.Connection = null;

    function HidePeekUI(Container)
    {
        generatePeek.ChartData = null;
        $(Container).removeClass("RevealPreviewPanel");
    }

    function RevealPeekUI(Container, PeekData)
    {
        $(Container).addClass("RevealPreviewPanel");
        /*
        ;
        var PeekDaysSampleData = [
                            { TotalDuration: 14, DurationRatio: 0.3, SleepTime: 4, DayIndex: 5 },
                            { TotalDuration: 12, DurationRatio: 0.5, SleepTime: 5, DayIndex: 6 },
                            { TotalDuration: 11, DurationRatio: 0.2, SleepTime: 5, DayIndex: 0 },
                            { TotalDuration: 15, DurationRatio: 0.7, SleepTime: 6, DayIndex: 1 },
                            { TotalDuration: 12, DurationRatio: 0.65, SleepTime: 7, DayIndex: 2 },
                            { TotalDuration: 10, DurationRatio: 0.48, SleepTime: 4, DayIndex: 3 },
                            { TotalDuration: 12, DurationRatio: 0.72, SleepTime: 8, DayIndex: 4 }
                       ]

        var PeekData = { PeekDays: PeekDaysSampleData, ConflctCount: 4 }*/
        var HighChartsData = {
            chart: {
                type: 'bar'
            },
            title: {
                text: 'Work Preview'
            },
            xAxis: {
                //categories: ['Apples', 'Oranges', 'Pears', 'Grapes', 'Bananas']
                categories: []
                
            },
            yAxis: {
                min: 0,
                title: {
                    //text: 'Total fruit consumption'
                    text: '24 Hours'
                },
                tickInterval: 3
            },
            legend: {
                reversed: true
            },
            plotOptions: {
                series: {
                    stacking: 'normal'
                }
            },
            series: [
                 {
                     name: 'Sleep',
                     //data: [2, 2, 3, 2, 1]
                     data: []
                 },
                {
                    name: 'Work',
                    //data: [5, 3, 4, 7, 2]
                    data: []
                }
            /*,
            {
                name: 'Joe',
                data: [3, 4, 4, 2, 5]
            }
            */
            ]
        }

        

        
        var labelData = [];
        var SleepData = [];
        var WorkData = [];
        var InitialDayOfWeek =PeekData.PeekDays[0].DayIndex;
        for (var i = 0; i < PeekData.PeekDays.length; i++)
        {
            labelData.push(WeekDays[(InitialDayOfWeek + i)%7]);
            SleepData.push(PeekData.PeekDays[i].SleepTime);
            WorkData.push(PeekData.PeekDays[i].TotalDuration);
        }
        HighChartsData.series[1].data = WorkData;
        HighChartsData.series[0].data = SleepData;
        HighChartsData.xAxis.categories = labelData;

        
        if (!generatePeek.ChartData)
        {
            setTimeout(function ()
            {
                var mydata1 = $(Container).highcharts(HighChartsData);
                generatePeek.ChartData = mydata1;
            }, 700)
            
        }
        else {
            
            generatePeek.ChartData.highcharts().series[1].setData(WorkData,true);
            generatePeek.ChartData.highcharts().series[0].setData(SleepData,true);
        }

        

        var startWithDataset = 1;
        var startWithData = 1;


    }
    

    
    
}

generatePeek.peekIsOn = false;
generatePeek.ChartData = null;

function generateAddEventContainer(x,y,height,Container,refStartTime)
{
    global_ExitManager.triggerLastExitAndPop();
    getRefreshedData.disableDataRefresh();
    ActivateUserSearch.setSearchAsOff();
    var NewEventcontainer = getDomOrCreateNew("AddNewEventContainer");

    $(NewEventcontainer.Dom).click(function (event) {//stops clicking of add event from propagating
        event.stopPropagation();
    });
    /*
    function removePanel(e)
    {
        //if (e.keyCode == 27)
        {
            getRefreshedData();
            CloseEventAddition();
            
        }
        e.stopPropagation();
    }
    */
    

    function CloseEventAddition()
    {
        //$(document).off("keyup", document, removePanel);
        if (NewEventcontainer != null) {
            if (NewEventcontainer.Dom.parentElement != null) {
                NewEventcontainer.Dom.parentElement.removeChild(NewEventcontainer.Dom);
            }
        }
        $(ColorPicker.Selector.Container).removeClass("ColorPickerContainerRigid");
        getRefreshedData.enableDataRefresh();
        ActivateUserSearch.setSearchAsOn();
    }
    //$(document).keyup(removePanel);

    

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
    var ColorPicker = generateColorPickerContainer();
    $(ColorPicker.Selector.Container).addClass("ColorPickerContainerRigid");
    var recurrence = createCalEventRecurrence();
    global_ExitManager.addNewExit(CloseEventAddition);
  //  var EnableTiler = generateTilerEnabled(EndDom.Selector.Container, SplitCount.Selector.Container);

    NewEventcontainer.Dom.appendChild(NameDom.Selector.Container);
    NewEventcontainer.Dom.appendChild(StartDom.Selector.Container);
    NewEventcontainer.Dom.appendChild(DurationDom.Selector.Container);
    //NewEventcontainer.Dom.appendChild(EndDom.Selector.Container);
    NewEventcontainer.Dom.appendChild(LocationDom.Selector.Container);

    NewEventcontainer.Dom.appendChild(recurrence.Content);
    NewEventcontainer.Dom.appendChild(ColorPicker.Selector.Container);

    NewEventcontainer.Dom.appendChild(SubmitButton.Selector.Container);
    
    
    
    
    

    $(SubmitButton.Selector.Button.Dom).click(function () {
        var NewEvent = BindSubmitClick(NameDom.Selector.Input.Dom.value, LocationDom.Selector.Address.Dom, LocationDom.Selector.NickName.Dom.value, SplitCount.Selector.Input.Dom.value, StartDom, EndDom, DurationDom, null, true, ColorPicker.Selector.getColor(), global_ExitManager.triggerLastExitAndPop, recurrence);
        SendScheduleInformation(NewEvent, global_ExitManager.triggerLastExitAndPop);
    })
    AddCloseButoon(NewEventcontainer, false);
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
    var HourSliderValue = getDomOrCreateNew("HourSliderValue", "input");
    HourSliderValue.setAttribute("type", "number")
    HourSliderValue.Dom.value=0;
    var HourLabel = getDomOrCreateNew("HourLabel", "span");
    HourLabel.Dom.innerHTML="H";
    var MinSliderValue = getDomOrCreateNew("MinSliderValue", "input");
    MinSliderValue.setAttribute("type", "number")
    MinSliderValue.Dom.value = 0;
    var MinLabel = getDomOrCreateNew("MinLabel", "span");
    MinLabel.Dom.innerHTML="M";
    var DaySliderValue = getDomOrCreateNew("DaySliderValue", "input");
    DaySliderValue.setAttribute("type","number")
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
    
    var AllElements = TileInptObject.getAllElements();
    for (var i = 0; i < AllElements.length; i++) {
        
        var myElement = AllElements[i];
        if (myElement != null)
        {
            Container.Dom.appendChild(myElement);
        }
    }
}

//Handles the activities of sliders. Sliders show up beneath the done button
function InactiveSlider(InActiveDom, ActiveDom, ButtonElements, AutoSentence)
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
    var LastElement = new TileInputBox(AllInputData[AllInputData.length - 1], AllInputDataContainer, undefined, global_ExitManager.triggerLastExitAndPop, undefined, null, AutoSentence);
    AllTileElements.push(LastElement);

    for (var i = AllInputData.length - 2, j = AllInputData.length - 1; i >= 0; i--, j--)
    {
        AllInputData[i].NextElement = LastElement;
        LastElement = new TileInputBox(AllInputData[i], AllInputDataContainer, undefined, global_ExitManager.triggerLastExitAndPop, undefined, null, AutoSentence);
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

    function turnOnButton()
    {
        ButtonSlide.SetAsOn();
    }

    this.turnOnSlide = turnOnButton;

    function turnOffButton() {
        ButtonSlide.SetAsOff();
    }

    this.turnOffSlide = turnOffButton

    function getContainer()
    {
        return AllInputDataContainer;
    }

    this.getContainer = getContainer;


    ButtonSlide.SetAsOff();
}

InactiveSlider.ID = 0;


function cleanUpTimeRestriction(TimeRestrictionSlider)
{
    var TimeRestrictionAllElements = TimeRestrictionSlider.getAllElements();
    //for (var i = 0; i < TimeRestrictionAllElements.length; i++)
    
    var StartInputLabel = TimeRestrictionAllElements[3].TileInput.getLabelBefore()
    var StartInputDom = TimeRestrictionAllElements[3].TileInput.getInputDom();
    BindTimePicker(StartInputDom);
    $(StartInputDom).addClass("TimeInput");
    StartInputDom.setAttribute("placeholder", "24 Hrs");
    StartInputDom.onkeypress = onKeyPress;
    
    var EndInputLabel = TimeRestrictionAllElements[4].TileInput.getLabelBefore()
    var EndInputDom = TimeRestrictionAllElements[4].TileInput.getInputDom();
    EndInputDom.onkeypress = onKeyPress;
    BindTimePicker(EndInputDom);
    $(EndInputDom).addClass("TimeInput");
    EndInputDom.setAttribute("placeholder", "24 Hrs");
    var WorkDayLabel = TimeRestrictionAllElements[1].TileInput.getLabelAfter()
    var WorkDayCheckBox = TimeRestrictionAllElements[1].TileInput.getInputDom()
    var EveryDayCheckBox = TimeRestrictionAllElements[0].TileInput.getInputDom()
    var WeekDayCheckBox = TimeRestrictionAllElements[2].TileInput.getInputDom()
    var WeekDayLabel = TimeRestrictionAllElements[2].TileInput.getLabelAfter();
    WeekDayCheckBox.onchange = onWeekDayCheckboxClick;
    

    
    var parentDom = WorkDayCheckBox.parentElement;
    var WeekDayDom = getDomOrCreateNew("WorkDayDom")
    var StartEndInutContainer = getDomOrCreateNew("StartEndInutContainer");
    $(StartEndInutContainer).addClass("setAsDisplayNone");

    StartEndInutContainer.appendChild(StartInputLabel);
    StartEndInutContainer.appendChild(StartInputDom);
    StartEndInutContainer.appendChild(EndInputLabel);
    StartEndInutContainer.appendChild(EndInputDom);


    WeekDayDom.appendChild(WeekDayCheckBox)
    WeekDayDom.appendChild(WeekDayLabel);
    parentDom.appendChild(WeekDayDom);
    parentDom.appendChild(StartEndInutContainer);
    WorkDayCheckBox.onchange = onCheckBoxChange;
    EveryDayCheckBox.onchange = onEverydayCheckBoxChange;


    function onKeyPress(e)
    {
        e.stopPropagation();
    }

    function onEverydayCheckBoxChange()
    {
        if (EveryDayCheckBox.checked)
        {
            WorkDayCheckBox.checked = false;
            showTimeInput();
            DisableWeekDay();
        }
        else
        {
            hideTimeInput()
        }
    }
    function onCheckBoxChange(e)
    {
        
        if (WorkDayCheckBox.checked)
        {
            triggerChangeInTime();
            EveryDayCheckBox.checked = false;
            EveryDayCheckBox.onchange();
            showTimeInput(); 
            DisableWeekDay();
        }
        else
        {
            hideTimeInput()
        }
        function triggerChangeInTime()
        {

            StartInputDom.value = "9:00 am"
            EndInputDom.value = "6:00 pm"
        }
    }
    

    function onWeekDayCheckboxClick(e)
    {
        
        if (WeekDayCheckBox.checked)
        {
            EveryDayCheckBox.checked = false;
            WorkDayCheckBox.checked = false;
            WorkDayCheckBox.onchange();
            EveryDayCheckBox.onchange();
            RestrictiveWeek.showWeekDayButtons();
        }
        else
        {
            DisableWeekDay();
            return;
        }
    }


    function showTimeInput()
    {
        $(StartEndInutContainer).removeClass("setAsDisplayNone");
    }

    function hideTimeInput()
    {
        $(StartEndInutContainer).addClass("setAsDisplayNone");
    }

    function WeekDayButton(IndexData)
    {
        var Index = IndexData;
        var Start = "";
        var End = "";
        var StartDom = null;
        var EndDom = null;
        var isInitialized = false;

        function updateStart(StartData)
        {
            Start = StartData
            isInitialized = initializationTest();
        }

        function updateEnd(EndData) {
            End = EndData
            isInitialized = initializationTest();
        }
        function reset()
        {
            Start = "";
            End = "";
            isInitialized = false;
        }

        function setStartDom(StartDomData)
        {
            StartDom = StartDomData;
            StartDom.onchange = function () {
                updateStart(StartDom.value);

            }
        }

        function setEndDom(EndDomData)
        {
            EndDom = EndDomData;
            EndDom.onchange = function()
            {
                updateEnd(EndDom.value);
            }
        }

        function getStartDom() {
            var RetValue = StartDom ;
            return RetValue;
        }

        function getEndDom() {
            var RetValue = EndDom;
            return RetValue;
        }

        function enableStartInput()
        {
            StartDom.disabled = false;
            Start = StartDom.value;
        }

        function enableEndInput() {
            EndDom.disabled = false;
            Start = StartDom.value;
        }

        function disableStartInput() {
            StartDom.disabled = true;
        }

        function disableEndInput() {
            EndDom.disabled = true;
        }

        function initializationTest()
        {
            var RetValue = false;
            RetValue = (Start && End)&&true;
            return RetValue;
        }
        function getStart()
        {
            return Start;
        }

        function getEnd()
        {
            return End;
        }

        function getDayName()
        {
            return WeekDays[Index];
        }

        function getPostData()
        {
            var RetValue = { Start: Start, End: End, Index: Index }
            return RetValue;
        }

        function disableInputs()
        {
            disableStartInput();
            disableEndInput();
        }

        function enableInputs() {
            enableStartInput();
            enableEndInput();
        }

        function isWeekdayInitialized() {
            return isInitialized;
        }

        this.getDayName = getDayName;
        this.disableStartInput = disableStartInput;
        this.disableEndInput = disableEndInput;
        this.enableStartInput = enableStartInput;
        this.enableEndInput = enableEndInput;

        this.disableInputs = disableInputs;
        this.enableInputs = enableInputs;
        this.isInitialized = isWeekdayInitialized;

        this.getEnd = getEnd;
        this.getStart = getStart; 
        this.setStart = updateStart;
        this.setEnd = updateEnd;
        this.reset = reset;
        this.getPostData = getPostData;
        this.setStartDom = setStartDom;
        this.setEndDom = setEndDom;
        this.getStartDom = getStartDom;
        this.getEndDom = getEndDom;
    }

    function RestrictiveWeekControl()
    {
        var WeekDayButtons = [];
        var isEnabled = false;
        InitializeWeekDayButtons();
        function InitializeWeekDayButtons()
        {
            for (var i = 0; i < WeekDayButtons.length; i++)
            {
                var meButton = WeekDayButtons[i];
                if (meButton.WeekDayButton.parentElement!=null)
                {
                    (meButton.WeekDayButton.parentElement.removeChild(meButton.WeekDayButton))
                    meButton.WeekDayButton = null;;
                }
                //WeekDayButtons.push(meButton);
            }
            WeekDayButtons = [];
            for (var i = 0; i < WeekDays.length; i++) {
                var meButton = { WeekDayButton: new WeekDayButton(i), UIElement: null };
                WeekDayButtons.push(meButton);
            }
        }
        
        function generateWeekDayButton()
        {
            var RestrictiveWeekDayButtonContainer = getDomOrCreateNew("RestrictiveWeekDayButtonContainer");
            $(RestrictiveWeekDayButtonContainer).empty();
            RestrictiveWeekControl.RestrictionWeekDayContainer.appendChild(RestrictiveWeekDayButtonContainer);
            var RestrictiveWeekDayButtonInputContainer = getDomOrCreateNew("RestrictiveWeekDayButtonInputContainer")
            RestrictiveWeekControl.RestrictionWeekDayContainer.appendChild(RestrictiveWeekDayButtonInputContainer);
            InitializeWeekDayButtons();


            function OnSelectWeekDayButton(index, isSelected)
            {
                var ButtonMe = WeekDayButtons[index].WeekDayButton;
                if (isSelected)
                {
                    ButtonMe.enableInputs();
                }
                else
                {
                    ButtonMe.disableInputs();
                    ButtonMe.reset();
                }
            }
            var weekButtons = generateDayOfWeekRepetitionDom(OnSelectWeekDayButton);
            if (RestrictiveWeekControl.RestrictionWeekDayContainer.status) {
                $(RestrictiveWeekControl.RestrictionWeekDayContainer).empty();
            }
            
            function genreateWeekInputConainer()
            {
                for (var i= 0 ; i<WeekDayButtons.length;i++)
                {
                    
                    var RestrictedWeekdayInputContainer = getDomOrCreateNew("RestrictedWeekdayInputContainer" + i);
                    $(RestrictedWeekdayInputContainer).addClass("RestrictedWeekdayInputContainer");
                    var StartRestrictedWeekdayInputContainer = getDomOrCreateNew("StartRestrictedWeekdayInputContainer" + i);
                    $(StartRestrictedWeekdayInputContainer).addClass("TimeRestrictedWeekdayInputContainer");
                    var StartRestrictedWeekdayInputContainerInput = getDomOrCreateNew("StartRestrictedWeekdayInputContainerInput" + i);
                    $(StartRestrictedWeekdayInputContainerInput).addClass("StartRestrictedWeekdayInputContainerInput");
                    $(StartRestrictedWeekdayInputContainerInput).addClass("RestrictedWeekdayInputContainerInput");
                    var StartRestrictedWeekdayInput = getDomOrCreateNew("StartRestrictedWeekdayInput" + i,"input");
                    $(StartRestrictedWeekdayInput).addClass("StartRestrictedWeekdayInput");
                    StartRestrictedWeekdayInputContainerInput.appendChild(StartRestrictedWeekdayInput);
                    StartRestrictedWeekdayInput.setAttribute("placeholder", "Start");;
                    StartRestrictedWeekdayInput.disabled = true;
                    BindTimePicker(StartRestrictedWeekdayInput);

                    var EndRestrictedWeekdayInputContainer = getDomOrCreateNew("EndRestrictedWeekdayInputContainer" + i);
                    $(EndRestrictedWeekdayInputContainer).addClass("TimeRestrictedWeekdayInputContainer");
                    var EndRestrictedWeekdayInputContainerInput = getDomOrCreateNew("EndRestrictedWeekdayInputContainerInput" + i);
                    $(EndRestrictedWeekdayInputContainerInput).addClass("EndRestrictedWeekdayInputContainerInput");
                    $(EndRestrictedWeekdayInputContainerInput).addClass("RestrictedWeekdayInputContainerInput");
                    var EndRestrictedWeekdayInput = getDomOrCreateNew("EndRestrictedWeekdayInput" + i, "input");
                    EndRestrictedWeekdayInput.setAttribute("placeholder", "End");
                    EndRestrictedWeekdayInput.disabled = true;
                    $(EndRestrictedWeekdayInput).addClass("EndRestrictedWeekdayInput");
                    EndRestrictedWeekdayInputContainerInput.appendChild(EndRestrictedWeekdayInput);
                    BindTimePicker(EndRestrictedWeekdayInput);
                    




                    EndRestrictedWeekdayInputContainer.appendChild(EndRestrictedWeekdayInputContainerInput);
                    StartRestrictedWeekdayInputContainer.appendChild(StartRestrictedWeekdayInputContainerInput);



                    RestrictedWeekdayInputContainer.appendChild(StartRestrictedWeekdayInputContainer);
                    RestrictedWeekdayInputContainer.appendChild(EndRestrictedWeekdayInputContainer);

                    $(RestrictedWeekdayInputContainer).addClass("RestrictedWeekdayInputContainer");
                    $(StartRestrictedWeekdayInputContainer).addClass("StartRestrictedWeekdayInputContainer");
                    $(EndRestrictedWeekdayInputContainer).addClass("EndRestrictedWeekdayInputContainer");
                    RestrictiveWeekDayButtonInputContainer.appendChild(RestrictedWeekdayInputContainer);

                    WeekDayButtons[i].WeekDayButton.setStartDom(StartRestrictedWeekdayInput);
                    WeekDayButtons[i].WeekDayButton.setEndDom(EndRestrictedWeekdayInput)
                }
            }

            for (var i = 0; i < weekButtons.AllDoms.length; i++)
            {
                var UIElement = weekButtons.AllDoms[i];
                RestrictiveWeekDayButtonContainer.Dom.appendChild(UIElement.Dom);
                WeekDayButtons[UIElement.DayOfWeekIndex].UIElement = UIElement;
            }


            //weekButtons.AllDoms.forEach(function (eachDom) {  });
            var ContainerDom = TimeRestrictionSlider.getContainer()
            ContainerDom.appendChild(RestrictiveWeekControl.RestrictionWeekDayContainer);
            genreateWeekInputConainer();
            weekButtons.RevealDayOfWeek();
            isEnabled = true;
        }

        function hideWeekDayButtons() {
            
            InitializeWeekDayButtons();
            var RestrictionWeekDayContainer = RestrictiveWeekControl.RestrictionWeekDayContainer;

            $(RestrictionWeekDayContainer).empty();
            if (RestrictionWeekDayContainer.Dom.parentElement != null) {
                (RestrictionWeekDayContainer.Dom.parentElement.removeChild(RestrictionWeekDayContainer.Dom));
            }

            isEnabled = false;
        }

        function getPostData()
        {
            var RetValue = {isEnabled:isEnabled, WeekDayOption:[]}
            if (isEnabled)
            {
                for (var i = 0; i < WeekDayButtons.length; i++)
                {
                    var meButton = WeekDayButtons[i].WeekDayButton;
                    if (meButton.isInitialized()) {
                        RetValue.WeekDayOption.push(meButton.getPostData());
                    }
                }
            }

            return RetValue;
        }

        function getWeekDayButton(Index)
        {
            var RetValue= WeekDayButtons[Index];
            return RetValue;
        }


        this.getWeekDayButton = getWeekDayButton;
        this.getPostData = getPostData;
        this.showWeekDayButtons = generateWeekDayButton;
        this.hideWeekDayButtons = hideWeekDayButtons;
    }

    RestrictiveWeekControl.RestrictionWeekDayContainer = getDomOrCreateNew("RestrictionWeekDayContainer");

    var RestrictiveWeek = new RestrictiveWeekControl();

    function DisableWeekDay()
    {
        RestrictiveWeek.hideWeekDayButtons();
        WeekDayCheckBox.checked = false;
    }

    TimeRestrictionSlider.getRestirctionPostData = function () { return RestrictiveWeek.getPostData();}

    TimeRestrictionSlider.getStart= function()
    {
        var retValue= StartInputDom.value;
        return retValue;
    }

    TimeRestrictionSlider.getEnd= function()
    {
        var retValue= EndInputDom.value;
        return retValue;
    }

    TimeRestrictionSlider.isWorkWeek = function ()
    {
        var retValue = WorkDayCheckBox.checked;
        return retValue;
    }
    TimeRestrictionSlider.isEveryDay = function ()
    {
        var retValue = EveryDayCheckBox.checked;
        return retValue;
    }

    
    TimeRestrictionSlider.getRestrictiveWeek = function () { return RestrictiveWeek ;};
    
    TimeRestrictionSlider.EnableEveryDayCheckBox = function () {
        EveryDayCheckBox.checked = true;
        onEverydayCheckBoxChange();
    }

    TimeRestrictionSlider.EnableWorkDayCheckBox = function () {
        WorkDayCheckBox.checked = true;
        onCheckBoxChange();
    }

    TimeRestrictionSlider.EnableWeekDayCheckBox = function () {
        WeekDayCheckBox.checked = true;
        onWeekDayCheckboxClick();
    }
    
}



function PopulateSliders(AcitveSection, InAcitveSection, AutoSentence)
{
    var RepetionSliderData = GenerateTileRepetition();
    var RepetitionSlider = new InactiveSlider(InAcitveSection.Dom, AcitveSection.Dom, RepetionSliderData, AutoSentence);
    var TimeRestriction = generateTimeRestriction();
    var TimeRestrictionSlider = new InactiveSlider(InAcitveSection.Dom, AcitveSection.Dom, TimeRestriction, AutoSentence);
    var AddressNickName = generateAddressNickName();
    var AddressNickNameSlider = new InactiveSlider(InAcitveSection.Dom, AcitveSection.Dom, AddressNickName, AutoSentence);
    cleanUpTimeRestriction(TimeRestrictionSlider);

    var RetValue = { RepetitionSlider: RepetitionSlider, TimeRestrictionSlider: TimeRestrictionSlider, AddressNickNameSlider: AddressNickNameSlider }

    return RetValue;
}

function generateAddressNickName()
{
    var PerElementData = {
        LabelAfter: "Nick Name", Message:
            {
                Index: 2,
                LoopBack: function (value) {
                    var message = "";
                    var invalidMessage = false;
                    if (value != "") {
                        {
                            message = ", AKA " + value;
                        }
                    }

                    return message;
                }
            }, DefaultText: "Address Nick Name",
    }

    var ButtonElements = [];
    ButtonElements.push(PerElementData);
    var InActiveMessage = "Give Location Nick Name?";
    var ActiveMessage = "Address Nick Name";
    var RetValue = { InActiveMessage: InActiveMessage, ActiveMessage: ActiveMessage, ButtonElements: ButtonElements }
    return RetValue;
}

function GenerateTileRepetition()
{
    var CountElementData = {
        LabelBefore: "I need to do this",
        InputType:"number",
        Message:
        {
            Index: 5,
            LoopBack: function (value) {
                var message = "";
                var invalidMessage = false;
                if (value != "")
                {
                    value = Number(value);
                    switch(value)
                    {
                        case 1:
                            {
                                value="once";
                            }
                            break;
                        case 2:
                            {
                                value = "twice";
                            }
                            break;

                        case 3:
                            {
                                value = "thrice";
                            }
                            break;
                        default:
                            {
                                if (typeof (value) === "number") {
                                    value = value + " times";
                                }
                                else
                                {
                                    invalidMessage = true;
                                }

                                
                            }
                            break;
                    }
                    if (!invalidMessage)
                    {
                        message = " I need to do this " + value;
                    }
                }

                return message;
            }
        }
    };
    
    var PerElementData = {
        LabelBefore: "times per", CustomType:{Type:"select",InnerHtml: "<option></option><option>Day</option><option>Week</option><option>Month</option><option>Year</option>"}, Message:
        {
            Index: 6,
            LoopBack: function (value) {
                var message = "";
                var invalidMessage = false;
                if (value != "")
                {
                    {
                        message = " per " + value;
                    }
                }

                return message;
            }
        }, DefaultText: "Day/Week/Month/Year" };
    //DropDown: { url: [{ repetition: "Day" }, { repetition: "Week" }, { repetition: "Month" }, { repetition: "Year" }, { repetition: "Decade" }], LookOut: "repetition" } 
    var ButtonElements = [];
    ButtonElements.push(CountElementData);
    ButtonElements.push(PerElementData);
    var InActiveMessage = "Repeatedly? Currently: No";
    var ActiveMessage = "Repeatedly";
    var RetValue = { InActiveMessage: InActiveMessage, ActiveMessage: ActiveMessage, ButtonElements: ButtonElements }
    return RetValue;
}

function generateTimeRestriction()
{
    var StartTime = { LabelBefore: "Start Time" };
    var EndTime = { LabelBefore: "End Time" };
    var WorkDays = { LabelAfter: "Only Work days and Work Hours", DoNothing: true, InputType: "checkbox" };
    var Everyday = { LabelAfter: "Everyday", DoNothing: true, InputType: "checkbox" };
    var Weekdays = { LabelAfter: "Show Weekdays", DoNothing: true, InputType: "checkbox" };
    var ButtonElements = [];
    ButtonElements.push(Everyday);
    ButtonElements.push(WorkDays);
    ButtonElements.push(Weekdays);
    ButtonElements.push(StartTime);
    ButtonElements.push(EndTime);
    
    
    var InActiveMessage = "Time Restrictions? Currently: No";
    var ActiveMessage = "Time Of Day Resrictions";
    var RetValue = { InActiveMessage: InActiveMessage, ActiveMessage: ActiveMessage, ButtonElements: ButtonElements }
    return RetValue;
}



//handles the whole addition of tiled events. Handles the UI component and tabbing
function AddTiledEvent()
{
    global_ExitManager.triggerLastExitAndPop();
    getRefreshedData.disableDataRefresh();
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
    

    function changeSummaryBackgroundColor(ColorData)
    {
        if (changeSummaryBackgroundColor.CurrentColor != null)
        {
            $(AutoSentence.getContainer()).removeClass(changeSummaryBackgroundColor.CurrentColor);
        }
        changeSummaryBackgroundColor.CurrentColor = ColorData.ColorClass;
        $(AutoSentence.getContainer()).addClass(changeSummaryBackgroundColor.CurrentColor);
    }
    changeSummaryBackgroundColor.CurrentColor = null;

    
    

    ActiveContainer.Dom.appendChild(ModalContentContainer.Dom);
    ActiveContainer.Dom.appendChild(ModalActiveOptionsContainer.Dom)
    
    

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
        var SummaryTitle = getDomOrCreateNew("SummaryContentAutoCompletion");
        SummaryTitle.innerHTML = "Summary";

        ModalSenetenceContainer.appendChild(SummaryTitle);
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
    var AutoSentenceCOntainer = AutoSentence.getContainer();
    var PreviewPanel = getDomOrCreateNew("PreviewPanel");

    var ColorPicker = generateColorPickerContainer(changeSummaryBackgroundColor, true);//this has to be placed after AutoSentence  initialization in order to ensure that changeSummaryBackgroundColor doesnt make a call to null in the function changeSummaryBackgroundColor
    ActiveContainer.Dom.appendChild(ColorPicker.Selector.Container);//ColorPicker.Selector.Container has to be inserted before the done button container to ensure insertion before done
    ActiveContainer.Dom.appendChild(ModalDoneContentContainer.Dom)
    $(ColorPicker.Selector.Container).addClass("HorizontalColorPickerContainerTiledEvent");


    modalTileEvent.Dom.appendChild(PreviewPanel);
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

    
    var SliderData = PopulateSliders(ModalActiveOptionsContainer, InActiveContainer, AutoSentence);
    var RepetionSlider = SliderData.RepetitionSlider;
    var TimeRestrictionSlider = SliderData.TimeRestrictionSlider;
    var NickNameSlider = SliderData.AddressNickNameSlider;
    
    
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
        },DefaultText: "Task"
    };



    function DayOfTheWeekControl()
    {
        function DisplayDaysOfTheWeek()
        {

        }

        function ButtonClickCallBack()
        {

        }
    }

    /*
    Function handles the call back for the autoSuggest Box of location
    */
    function LocationSearchCallBack(ExitCallBack, InputBox)
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
            /*
            $(DomContainer).empty();
            if (data.length == 0 || data.length == null || data.length == undefined || (document.activeElement != InputBox))
            {
                ReseAutoSuggest();
                return;
            }

            */
            

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
                    NickNameSlider.turnOnSlide();
                    
                    var NickElements = NickNameSlider.getAllElements()
                    NickElements[0].TileInput.getInputDom().value = LocationData.Tag;
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
                        
                        resolveEachRetrievedEvent(results[i],i);
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

            function resolveEachRetrievedEvent(LocationData)//, Index)
            {
                var CalendarEventDom = generateDomForEach(LocationData)//, Index);
                $(CalendarEventDom.Container).addClass("LocationCacheContainer");
                //DomContainer.Dom.appendChild(CalendarEventDom.Container);
                GoogleDataContainer.AllData.push(CalendarEventDom);
                ++resolveEachRetrievedEvent.ID;
            }
            resolveEachRetrievedEvent.ID = 0;
            //GoogleDataContainer.AllData[0].Hover();

            function generateDomForEach(LocationData)//, Index)
            {
                var RestrictiveWeekSlider = TimeRestrictionSlider;
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
                    NickNameSlider.turnOnSlide();
                    
                    var NickElements = NickNameSlider.getAllElements();
                    getBusinessHourData(LocationData);
                    NickElements[0].TileInput.getInputDom().value = LocationData.name;
                    setTimeout(function () { InputBox.focus(), 200 });
                }

                function getBusinessHourData(LocationData)
                {
                    ;
                    var request = {
                        placeId: LocationData.place_id
                    };
                    var service = new google.maps.places.PlacesService(DomContainer);
                    service.getDetails(request, LocationUpdateCallBack);
                    function LocationUpdateCallBack(place, status)
                    {
                        if (status == google.maps.places.PlacesServiceStatus.OK) {
                            var RestrictedTimeData = generateOfficeHours(place);
                            if((!RestrictedTimeData.IsTwentyFourHours)&&(!RestrictedTimeData.NoWeekData))
                            {
                                RestrictiveWeekSlider .turnOnSlide();
                                RestrictiveWeekSlider.EnableWeekDayCheckBox();
                                var RestrictiveWeek = RestrictiveWeekSlider.getRestrictiveWeek();
                                for (var i=0;i<RestrictedTimeData.WeekDayData.length;i++)
                                {
                                    var myDay = RestrictedTimeData.WeekDayData[i];
                                    if (!myDay.IsClosed)
                                    {
                                        var myButton = RestrictiveWeek.getWeekDayButton(myDay.DayIndex);
                                        var Start = myDay.Start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                                        var End = myDay.End.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                                        myButton.UIElement.TurnOnButton();
                                        myButton.WeekDayButton.getStartDom().value = Start;
                                        myButton.WeekDayButton.setStart(Start);
                                        myButton.WeekDayButton.getEndDom().value = End;
                                        myButton.WeekDayButton.setEnd(End);
                                    }
                                }
                                setTimeout(function () { InputCOntainer.focus(); });//need to return focus to auto suggest input box. After selecting element system seems to shift focus to checkbox. 
                            }
                        }
                    }
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
                    ;
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
        DefaultText: "Location", DropDown: LocationSearchCallBack
    };
    
    var Hour = new TileInputBox({
        LabelAfter: "Hr",InputType:"number", Message: {
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
    }, undefined, undefined, global_ExitManager.triggerLastExitAndPop, undefined, null, AutoSentence)
    var Min = new TileInputBox({
        LabelAfter: "Min ", InputType: "number", Message: {
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
    }, undefined, undefined, global_ExitManager.triggerLastExitAndPop, undefined, null, AutoSentence)
    var Day = new TileInputBox({ LabelAfter: "D", InputCss: { width: "1em" } }, undefined, undefined, global_ExitManager.triggerLastExitAndPop)
    //var AllTimeLements = [Hour, Min, Day];
    var AllTimeLements = [Hour, Min];


    var Element3 = {
        LabelBefore: "It will Take",
        Message: {
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
            Index: 7,
            LoopBack: function (value) {
                var message = "";
                if (value != "") {

                    value = new Date(value).toDateString();;
                    message = " and I need to get it done by " + value;
                }

                return message;
            }
        }, DefaultText: "Deadline", TriggerDone: true
    };
    
    InActiveContainer.Hide();


    function Exit()///forces the removal of the Div
    {
        getRefreshedData.enableDataRefresh();
        if (modalTileEvent != null)
        {
            $(modalTileEvent.Dom).empty();
            if (modalTileEvent.Dom.parentElement!=null)
            {
                (modalTileEvent.Dom.parentElement.removeChild(modalTileEvent.Dom));
            }
        }
        $(ColorPicker.Selector.Container).removeClass("HorizontalColorPickerContainerTiledEvent");
        ActivateUserSearch.setSearchAsOn();
    }

    global_ExitManager.addNewExit(Exit);

    var AllTileElements = [];

    TileInputBox.DoneButton = new TileDoneButton(InActiveContainer);//creates a done button and makes it a static member of TileInputBox.
    TileInputBox.DoneButton.GetDom().onkeypress =(
        function (e) {
            if (e.which == 13) {
                SendData()
            }
        })

    
    AddTiledEvent.Exit = global_ExitManager.triggerLastExitAndPop;
    AllInputData.push(Element1);
    AllInputData.push(Element2);
    AllInputData.push(Element3);
    AllInputData.push(Element4);
    var LastElement = new TileInputBox(AllInputData[AllInputData.length - 1], ModalContentContainer, DoneButton, global_ExitManager.triggerLastExitAndPop, undefined, null, AutoSentence);
    AllTileElements.push(LastElement);
    
    for (var i = AllInputData.length-2, j = AllInputData.length - 1; i >= 0; i--, j--)
    {
        AllInputData[i].NextElement = LastElement;
        LastElement = new TileInputBox(AllInputData[i], ModalContentContainer, DoneButton, global_ExitManager.triggerLastExitAndPop, undefined, null, AutoSentence);
        AllTileElements.push(LastElement);
    }

    var BoundTimePicerObj = BindDatePicker(TileInputBox.Dictionary[Element4.ID].Me.getInputDom());//Set inbox as date time picker box
    

    BoundTimePicerObj.on("show", function () {
        //alert("show triggered");
        var keyEntryFunc = TileInputBox.Dictionary[Element4.ID].Me.getKeyCallBackFunc()
        var EndTimeInput = TileInputBox.Dictionary[Element4.ID].Me.getInputDom()
        
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
        var myColor = ColorPicker.Selector.getColor();
        var SendIt = prepSendTile(Element1.TileInput.getInputDom(), Element2.TileInput.getInputDom(), NickNameSlider, Splits.getInputDom(), Hour.getInputDom(), Min.getInputDom(), Element4.TileInput.getInputDom(), RepetionChoice.getInputDom(), RepetionSlider.getStatus(), myColor, TimeRestrictionSlider);
        if (TileInputBox.DoneButton.getStatus()) {
            SendIt();
            //AddTiledEvent.Exit();
        }
        else
        {
            alert("please provide viable deadline");
        }
    }

    var repetitionCount = RepetionSlider.getAllElements()[0].TileInput.getInputDom();
    repetitionCount.addEventListener("input", peekData);
    var HourInputDOM = Hour.getInputDom();
    HourInputDOM.addEventListener("input", peekData);
    var MinInputDOM = Min.getInputDom();
    MinInputDOM.addEventListener("input", peekData);
    var DeadlineInput = Element4.TileInput.getInputDom()
    $(DeadlineInput).datepicker().on("changeDate", peekData);
    //DeadlineInput.addEventListener("input", peekData);
    var frequencyInput = RepetionSlider.getAllElements()[1].TileInput.getInputDom();
    frequencyInput.addEventListener("input", peekData);

    function peekData()
    {
        var Splits = RepetionSlider.getAllElements()[0].TileInput;
        var RepetionChoice = RepetionSlider.getAllElements()[1].TileInput;
        var myColor = ColorPicker.Selector.getColor();
        var restrictionData = generatePostBackDataForTimeRestriction(TimeRestrictionSlider);
        
        var peekEvent = SubmitTile(Element1.TileInput.getInputDom().value, {},"", Splits.getInputDom().value, Hour.getInputDom().value, Min.getInputDom().value, Element4.TileInput.getInputDom().value, RepetionChoice.getInputDom().value, myColor, RepetionSlider.getStatus(), restrictionData);
        setTimeout(function () { generatePeek(peekEvent, PreviewPanel) }, 300);
    }


    /*
    function UIAddTileUITrigger(e)
    {
        if (e.which == 27)//escape key press
        {
            document.removeEventListener("keydown", UIAddTileUITrigger);
            AddTiledEvent.Exit()
        }
        
    }
    */
    
    TileInputBox.Send = SendData;
    
    $(TileInputBox.DoneButton.GetDom()).click(SendData);
    ModalDoneContentContainer.Dom.appendChild(TileInputBox.DoneButton.GetDom());

    InvisiblePanel.Dom.appendChild(modalTileEvent.Dom);
    //document.addEventListener("keydown", UIAddTileUITrigger);
    
    FirstElement.reveal();
    FirstElement.forceFocus();
    AddCloseButoon(modalTileEvent, true);
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
    LabelBefore+=" "
    var LabelAfter = TabElement.LabelAfter == null ? "" : TabElement.LabelAfter;
    var myTabElement = TabElement;
    var myTIleInputID = TileInputBox.ID++
    var InputBoxLabelBeforeID = "InputBoxLabelBefore" + myTIleInputID;
    var InputBoxLabelAfterID = "InputBoxLabelAfter" + myTIleInputID;
    var MyID = "TileInputID" + myTIleInputID;
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
    var InputBoxID = "InputBox" + myTIleInputID;
    this.NextElement = NextElement.Data;
    NextElement.Previous = this;
    var InputBox ;//= getDomOrCreateNew(InputBoxID, "input");
    if (!TabElement.CustomType)
    {
        InputBox = getDomOrCreateNew(InputBoxID, "input");
    }
    else
    {
        InputBox = getDomOrCreateNew(InputBoxID,TabElement.CustomType.Type);
        InputBox.innerHTML = TabElement.CustomType.InnerHtml;
    }
    var InputDataDomain = InputBox;
    InputDataDomain.CleanUp = function ()
    {
        return;
    }
    
    var labelAndInputContainerID = "labelAndInputContainer" + myTIleInputID;;
    var labelAndInputContainer = getDomOrCreateNew(labelAndInputContainerID);

    
    var invisibleSpan = getDomOrCreateNew("measureSpan" + myTIleInputID, "span");
    $(invisibleSpan.Dom).addClass("invisibleSpan");
    //labelAndInputContainer.Dom.appendChild(invisibleSpan.Dom);
    $(InputBox.Dom).addClass("TileInput");

    //$(labelAndInputContainer.Dom).addClass("NonReveal");
    //$(labelAndInputContainer.Dom).addClass("labelAndInputContainer");
    
    var OtherElements = [];
    
    var AutoSuggestFunction = null;
    

    //fuction generates and binds all elements for a drop down menu option
    function GenerateAutoSuggest()
    {
        var dropDown;
        var JSONProperty;
        if (TabElement.DropDown != undefined)
        {
            AutoSuggestFunction = TabElement.DropDown(Exit, InputBox.Dom);
        }
        /*
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
            $(Container).removeClass("setAsDisplayNone");
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
                $(SuggestedValueContainer).addClass("setAsDisplayNone");
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
        */
    }

    function GenerateAlreadyCreatedBoxes()
    {
        //
        if (TabElement.SubTileInputBox != undefined)
        {
            
            TabElement.SubTileInputBox.forEach(revealEachElement);
        }

        function revealEachElement(eachSubTileInputBox)
        {
            
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
        return;
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

        var retValue = [InputBoxLabelBefore, InputBox, InputBoxLabelAfter, InputDataDomain.Dom, invisibleSpan];
        if (InputBox == InputDataDomain)
        {
            retValue = [InputBoxLabelBefore, InputBox, InputBoxLabelAfter, invisibleSpan];
        }
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
        if (!(value.trim()))
        {
            $(InputBox.Dom).addClass("EmptyTileInput");
            return 0;
        }

        $(InputBox.Dom).removeClass("EmptyTileInput");
        var span = invisibleSpan.Dom;
        span.innerHTML = value;
        var span_width = $(span).width() + 20;
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


        if (TabElement.DropDown != undefined) {
            AutoSuggestFunction(e, Exit);
            return;
        }


        
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
            Exit()
        }

        /*
        if ((e.which == 38))
        {
            if (TabElement.DropDown != undefined)
            {
                TabElement.DropDown.OnUpKey()
            }
        }*/

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
        $(InputBox.Dom).removeClass("OutFocusTileInputBox ")
        tabfunction();
        InputBox.Dom.removeEventListener("keydown", KeyEntry);
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
        $(InputBox.Dom).addClass("OutFocusTileInputBox ")
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

    if (TabElement.InputType != undefined)
    {
        (InputBox).setAttribute("type", TabElement.InputType)
    }
    function focusInputBox()
    {
        if(InputBox!=null)
        {
            InputBox.focus();
        }
    }


    if (!TabElement.DoNothing) {
        GenerateAutoSuggest();
        GenerateAlreadyCreatedBoxes();
        DeployInputSettings();
        unReveal();
    }

    
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
    
    InputBoxLabelBefore.onclick = focusInputBox
    InputBoxLabelAfter.onclick = focusInputBox
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
    $(SubmitButton.Dom).addClass('SubmitButton');
    SubmitButtonContainer.Dom.appendChild(SubmitButton.Dom);
    SubmitButtonContainer.Selector = { Container: SubmitButtonContainer.Dom, Button: SubmitButton };
    return SubmitButtonContainer;
}


function BindSubmitClick(Name, AddressDom, AddressNick, Splits, Start, End, EventNonRigidDurationHolder, RepetitionEnd, RigidFlag, CalendarColor,ExitAdditionScreen,EventRepetition)
{
    var EventLocation = new Location(AddressNick, AddressDom.Dom.value.AddressDom.Dom.LocationIsVerified, AddressDom.LocationId);
    var EventName = Name;
    var EventName = Name;
    /*
    if (!EventName) {
        alert("Oops your Event needs a name");
        return null;
    }
    */
    
    if (Splits == "")
    {
        Splits = 1;
    }
    Splits = 1;
    var EventDuration = EventNonRigidDurationHolder.Selector.TimeHolder();
     //CalendarColor = { r: 200, g: 200, b: 200,a:1,selection:0 };
    CalendarColor = { r: CalendarColor.r, g: CalendarColor.g, b: CalendarColor.b, s: CalendarColor.Selection, o: CalendarColor.a };

    var EventStart = Start.getDateTimeData();
    var EventEnd = End.getDateTimeData();
    var DurationInMS = (parseInt((!EventDuration.Days) ? 0 : EventDuration.Days) * OneDayInMs) + (parseInt((!EventDuration.Hours) ? 0 : EventDuration.Hours) * OneHourInMs) + (parseInt((!EventDuration.Mins) ? 0 : EventDuration.Mins) * OneMinInMs)
    /*
    if (DurationInMS == 0) {
        alert("Oops please provide a duration for \"" + EventName + "\"");
        return null;
    }
    */

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
    var RepetitionEnd = EventRepetition.RepeatEnd.value;

    if (EventRepetition.RepetitionStatus.status) {
        EventRepetition.RepetitionSelection.forEach(function (Selection)
        {
            Selection.Dom = null;
            if (Selection.status)
            {
                repeteOpitonSelect = Selection;
            }
        });
    }

    var NewEvent = new CalEventData(EventName, EventLocation, Splits, CalendarColor, EventDuration, EventStart, EventEnd, repeteOpitonSelect, RepetitionStart, RepetitionEnd, RigidFlag);
    NewEvent.RepeatData = null;
    if (NewEvent == null) {
        return;
    }
    
    return NewEvent;
}



function SendScheduleInformation(NewEvent, CallBack)
{
    //var url = "RootWagTap/time.top?WagCommand=1"
    
    //NewEvent = null;
    var ErrorCheck = isCalEvenValidForSend(NewEvent)
    if (ErrorCheck.isError)
    {
        alert(ErrorCheck.ErrorMessage);
        return;
    }

    NewEvent.UserName = UserCredentials.UserName
    NewEvent.UserID = UserCredentials.ID;
    var TimeZone = new Date().getTimezoneOffset();
    NewEvent.TimeZoneOffset = TimeZone;
    NewEvent.TimeZone = moment.tz.guess()
    var url = global_refTIlerUrl + "Schedule/Event";
    preSendRequestWithLocation(NewEvent)
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
            triggerUndoPanel("Undo addition of \"" + NewEvent.Name+"\"");
            var b = 3;


        },
        error: function (err) {
            //var myError = err;
            //var step = "err";
            var NewMessage = "Oh No!!! Tiler is having issues modifying your schedule. Please try again Later :(";
            var ExitAfter = { ExitNow: true, Delay: 1000 };
            HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, function () { });
        }

    }).done(function (response) {
        HandleNEwPage.Hide();
        getRefreshedData.enableDataRefresh();
        ;
        var AffirmCallBack = affirmNewEvent(response);
        
        getRefreshedData(AffirmCallBack);
        CallBack();
    });
}

function isCalEvenValidForPeek(CalEvent)
{
    var Result = { isError: false, ErrorMessage: "" };
    var TotalDuration = getTotalDurationFromCalEvent(CalEvent);
    var EndDate = getCalEventEnd(CalEvent);

    if (!(TotalDuration > 0)) {
        Result.isError = true;
        Result.ErrorMessage = "You havent set the duration for your tile";
        return Result;
    }
    if (!isDateValid(EndDate)) {
        Result.isError = true;
        Result.ErrorMessage = "Please provide the deadline for your event";
        return Result;
    }

    if (!(CalEvent.Count > 0)) {
        Result.isError = true;
        Result.ErrorMessage = "Your tile has an invalid Repetition Count";
        return Result;
    }

    return Result;
}

function isCalEvenValidForSend(CalEvent)
{
    var Result = { isError: false, ErrorMessage: "" };
    var TotalDuration = getTotalDurationFromCalEvent(CalEvent);
    var EndDate = getCalEventEnd(CalEvent);
    
    if (!(TotalDuration > 0)) 
    {
        Result.isError = true;
        Result.ErrorMessage = "You havent set the duration for your tile";
        return Result;
    }
    if(!isDateValid(EndDate))
    {
        Result.isError = true;
        Result.ErrorMessage = "Please provide the deadline for your event";
        return Result;
    }
    var Name = CalEvent.Name;
    
    if (Name!=null)
    {
        Name=Name.trim()
        if (Name == "")
        {
            Result.isError = true;
            Result.ErrorMessage = "Your tile needs a name";
            return Result;
        }   
    }
    else
    {
        Result.isError = true;
        Result.ErrorMessage = "Your tile has an invalid name";
        return Result;
        
    }

    if (!(CalEvent.Count > 0))
    {
        Result.isError = true;
        Result.ErrorMessage = "Your tile has an invalid Repetition Count";
        return Result;
    }


    return Result;
}



function affirmNewEvent(response)
{
    var StartOfEvent=null;
    var EventID = null;
    function retValue(CallBack)
    {
        if (StartOfEvent != null)
        {
            if (global_GoToDay(StartOfEvent))
            {
                setTimeout(function () {
                    //renderSubEventsClickEvents(EventID);

                    
                    //if (true)
                    {
                        
                        var CurrentWeekContainer = $(getDomOrCreateNew("CurrentWeekContainer"));
                        var TimeSizeDom = $(global_DictionaryOfSubEvents[EventID].TimeSizeDom)
                        var bar = $(TimeSizeDom).offset().top - $(CurrentWeekContainer).offset().top
                        var WidthInPixels = bar;
                        //$("#NameOfWeekContainerPlane").animate({ scrollTop: WidthInPixels }, 1000);
                        $("#CurrentWeekContainer").animate({ scrollTop: WidthInPixels }, 1000);
                    }
                    
                    global_UISetup.RenderOnSubEventClick(EventID);
                }, 1500);
                
            }
            else
            {
                populateMonth(StartOfEvent, retValue);
            }
        }
        if(CallBack!=null)
        {
            CallBack();
        }
    }
    if (response.Error.code == 0)
    {
        StartOfEvent = new Date(response.Content.SubCalCalEventStart);
        EventID = response.Content.ID;

        /*
        if (global_GoToDay(StartOfEvent))
        {
            setTimeout(function () { renderSubEventsClickEvents(EventID); }, 5000);
            return;
        }
        else {
            populateMonth(StartOfEvent);
            //setTimeout(retValue, 12000);
        }*/
    }

    return retValue;
}

function createCalEventRecurrence()
{
    
    var RecurrenceTabContentID = "RecurrenceTabContent";
    var RecurrenceTabContent = getDomOrCreateNew(RecurrenceTabContentID);
    RecurrenceTabContent.Misc = { Selection: null };
    var EventrepeatStatus;
    var EventRepetitionSelection;
    var EventRepeatStart;
    var EventRepeatEnd;
    
    $(RecurrenceTabContent.Dom).addClass("TabContent");

    //Enable Recurrence
    var EnableRecurrenceContainerID = "EnableRecurrenceContainer";
    var EnableRecurrenceContainer = getDomOrCreateNew(EnableRecurrenceContainerID);

    var EnableRecurrenceLabelID = "EnableRecurrenceLabel";
    var EnableRecurrenceLabel = getDomOrCreateNew(EnableRecurrenceLabelID,"label");
    EnableRecurrenceContainer.Dom.appendChild(EnableRecurrenceLabel.Dom);
    $(EnableRecurrenceContainer.Dom).addClass(CurrentTheme.FontColor);
    EnableRecurrenceLabel.Dom.innerHTML = "Do you want this event to recurr?<br/> <span class='PressSpacebar'>Press Spacebar to toggle and/or to select a color below.</span>"

    /*var EnableRecurrenceButtonContainerID = "EnableRecurrenceButtonContainer";
    var EnableRecurrenceButtonContainer = getDomOrCreateNew(EnableRecurrenceButtonContainerID);


    EnableRecurrenceContainer.Dom.appendChild(EnableRecurrenceButtonContainer.Dom);
    $(EnableRecurrenceButtonContainer.Dom).addClass("EnableButtonContainer");*/

    var EnableRecurrenceButtonID = "EnableRecurrenceButton";
    var EnableRecurrenceButton = generateMyButton(ButtonClick, EnableRecurrenceButtonID);// getDomOrCreateNew(EnableRecurrenceButtonID);
    $(EnableRecurrenceButton.Dom).addClass("EnableButton");
    EnableRecurrenceContainer.Dom.appendChild(EnableRecurrenceButton);
    EnableRecurrenceButton.status = 0;
    EventrepeatStatus = EnableRecurrenceButton;


    //Enabled Recurring Settings
    var EnabledRecurrenceContainerID = "EnabledRecurrenceContainer";
    var EnabledRecurrenceContainer = getDomOrCreateNew(EnabledRecurrenceContainerID);


    var EnableRecurrenceYesTextID = "EnableRecurrenceYesText";
    /*var EnableRecurrenceYesText = getDomOrCreateNew(EnableRecurrenceYesTextID);
    EnableRecurrenceButtonContainer.Dom.appendChild(EnableRecurrenceYesText.Dom);
    $(EnableRecurrenceYesText.Dom).addClass("EnableButtonChoiceText");
    $(EnableRecurrenceYesText.Dom).addClass("EnableButtonChoiceYeaText");


    var EnableRecurrenceNoTextID = "EnableRecurrenceNoText";
    var EnableRecurrenceNoText = getDomOrCreateNew(EnableRecurrenceNoTextID);
    EnableRecurrenceButtonContainer.Dom.appendChild(EnableRecurrenceNoText.Dom);
    //EnableRecurrenceButtonContainer.Dom.appendChild(EnableRecurrenceButton.Dom);//appending after because of z-index effect
    $(EnableRecurrenceNoText.Dom).addClass("EnableButtonChoiceText");
    $(EnableRecurrenceNoText.Dom).addClass("EnableButtonChoiceNayText");*/

    RecurrenceTabContent.Dom.appendChild(EnableRecurrenceContainer.Dom);

//    $(EnableRecurrenceContainer.Dom).click(genFunctionForButtonClick(EnableRecurrenceButton, EnabledRecurrenceContainer));
    EventrepeatStatus = EnabledRecurrenceContainer;

    EnableRecurrenceButton.SetAsOff();

    
    function ButtonClick () 
    {
        switch (EnableRecurrenceButton.status) {
            case 0:
                {
                    $(EnabledRecurrenceContainer.Dom).hide();
                    EnabledRecurrenceContainer.status = 0;
                }
                break;
            case 1:
                {
                    $(EnabledRecurrenceContainer.Dom).show();
                    EnabledRecurrenceContainer.status = 1;
                }
                break;
        }
    }
    






    /*Recurrence Button Start*/
    var RecurrenceButtonContainerID = "RecurrenceButtonContainer"
    var RecurrenceButtonContainer = getDomOrCreateNew(RecurrenceButtonContainerID);

    var dailyRecurrenceButtonID = "dailyRecurrenceButton";
    var dailyRecurrenceButton = getDomOrCreateNew(dailyRecurrenceButtonID,"button");
    dailyRecurrenceButton.Range = OneDayInMs;
    dailyRecurrenceButton.Type = { Name: "Daily", Index: 0 };
    dailyRecurrenceButton.Misc = null;
    var dailyRecurrenceButtonTextID = "dailyRecurrenceButtonText";
    var dailyRecurrenceButtonText = getDomOrCreateNew(dailyRecurrenceButtonTextID);
    dailyRecurrenceButtonText.Dom.innerHTML = "Daily"
    $(dailyRecurrenceButton.Dom).addClass("recurrenceButton");
    $(dailyRecurrenceButton.Dom).addClass("SubmitButton")
    $(dailyRecurrenceButtonText.Dom).addClass("CentreAlignedName");
    $(dailyRecurrenceButton.Dom).append(dailyRecurrenceButtonText.Dom);


    var weeklyRecurrenceButtonID = "weeklyRecurrenceButton";
    var weeklyRecurrenceButton = getDomOrCreateNew(weeklyRecurrenceButtonID, "button");
    weeklyRecurrenceButton.Range = OneWeekInMs;
    weeklyRecurrenceButton.Type = { Name: "Weekly", Index: 1 };
    weeklyRecurrenceButton.Misc = null;
    var weeklyRecurrenceButtonTextID = "weeklyRecurrenceButtonText";
    var weeklyRecurrenceButtonText = getDomOrCreateNew(weeklyRecurrenceButtonTextID);
    $(weeklyRecurrenceButton.Dom).addClass("recurrenceButton");
    $(weeklyRecurrenceButton.Dom).addClass("SubmitButton")
    $(weeklyRecurrenceButtonText.Dom).addClass("CentreAlignedName");
    weeklyRecurrenceButtonText.Dom.innerHTML = "Weekly"
    $(weeklyRecurrenceButton.Dom).append(weeklyRecurrenceButtonText.Dom);


    var monthlyRecurrenceButtonID = "monthlyRecurrenceButton";
    var monthlyRecurrenceButton = getDomOrCreateNew(monthlyRecurrenceButtonID, "button");
    monthlyRecurrenceButton.Range = FourWeeksInMs;
    monthlyRecurrenceButton.Type = { Name: "Monthly", Index: 2 };
    var monthlyRecurrenceButtonTextID = "monthlyRecurrenceButtonText";
    var monthlyRecurrenceButtonText = getDomOrCreateNew(monthlyRecurrenceButtonTextID);
    $(monthlyRecurrenceButton.Dom).addClass("recurrenceButton");
    $(monthlyRecurrenceButton.Dom).addClass("SubmitButton")
    monthlyRecurrenceButtonText.Dom.innerHTML = "Monthly"
    $(monthlyRecurrenceButtonText.Dom).addClass("CentreAlignedName");
    $(monthlyRecurrenceButton.Dom).append(monthlyRecurrenceButtonText.Dom);


    var yearlyRecurrenceButtonID = "yearlyRecurrenceButton";
    var yearlyRecurrenceButton = getDomOrCreateNew(yearlyRecurrenceButtonID, "button");
    yearlyRecurrenceButton.Range = OneYearInMs;
    yearlyRecurrenceButton.Type = { Name: "Yearly", Index: 3 };
    yearlyRecurrenceButton.Misc = null;
    var yearlyRecurrenceButtonTextID = "yearlyRecurrenceButtonText";
    var yearlyRecurrenceButtonText = getDomOrCreateNew(yearlyRecurrenceButtonTextID);
    $(yearlyRecurrenceButton.Dom).addClass("recurrenceButton");
    $(yearlyRecurrenceButton.Dom).addClass("SubmitButton")
    yearlyRecurrenceButtonText.Dom.innerHTML = "Yearly"
    $(yearlyRecurrenceButtonText.Dom).addClass("CentreAlignedName");
    $(yearlyRecurrenceButton.Dom).append(yearlyRecurrenceButtonText.Dom);

    /*
    $(dailyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateContentSection);
    $(weeklyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateContentSection);
    $(monthlyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateContentSection);
    $(yearlyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateContentSection);

    $(dailyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateFontColor);
    $(weeklyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateFontColor);
    $(monthlyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateFontColor);
    $(yearlyRecurrenceButton.Dom).addClass(CurrentTheme.AlternateFontColor);*/

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
    BussinessHourRadioLabel.Dom.innerHTML = "Business Hours";


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


    DayPreferenceContainer.Dom.appendChild(BussinessHourInputContainer.Dom)

    DayPreferenceContainer.Dom.appendChild(AnyHourTimeContainer.Dom)

    DayPreferenceContainer.Dom.appendChild(CustomHourTimeContainer.Dom)

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

    //RepetitionRangeCOntainer.Dom.appendChild(RepetitionRangeContainerStartContainer.Dom);


    var RepetitionRangeContainerEndContainerID = "RepetitionRangeContainerEndContainer";
    var RepetitionRangeContainerEndContainer = getDomOrCreateNew(RepetitionRangeContainerEndContainerID);
    var RepetitionRangeContainerEndContainerLabelID = "RepetitionRangeContainerEndContainerLabel";
    var RepetitionRangeContainerEndContainerLabel = getDomOrCreateNew(RepetitionRangeContainerEndContainerLabelID,"label");
    RepetitionRangeContainerEndContainerLabel.Dom.innerHTML = "Repeat End:";
    $(RepetitionRangeContainerEndContainerLabel.Dom).addClass("DateInputLabel");

    var RepetitionRangeContainerEndInputID = "RepetitionRangeContainerEndInput";
    var RepetitionRangeContainerEndInput = getDomOrCreateNew(RepetitionRangeContainerEndInputID, "input");


    $(RepetitionRangeContainerEndContainer.Dom).append(RepetitionRangeContainerEndContainerLabel.Dom);
    $(RepetitionRangeContainerEndContainer.Dom).append(RepetitionRangeContainerEndInput.Dom);
    $(RepetitionRangeContainerEndInput.Dom).addClass("DateInputData");
    //$(RepetitionRangeContainerEndInput.Dom).datepicker();
    EventRepeatEnd = RepetitionRangeContainerEndInput;
    BindDatePicker(RepetitionRangeContainerEndInput);
    RepetitionRangeContainerEndInput.onkeyup = (function (e) { e.stopPropagation;})

    RepetitionRangeCOntainer.Dom.appendChild(RepetitionRangeContainerEndContainer.Dom);

    /*Repeat event container End*/

    var RecurrenceSectionCompletionID = "RecurrenceSectionCompletion";
    var RecurrenceSectionCompletion = getDomOrCreateNew(RecurrenceSectionCompletionID);
    $(RecurrenceSectionCompletion.Dom).addClass("SectionCompletionButton");
    $(RecurrenceSectionCompletion.Dom).addClass(CurrentTheme.AlternateContentSection);



    /*Recurrence Day Preference Container End*/
    EnabledRecurrenceContainer.Dom.appendChild(RecurrenceButtonContainer.Dom)
    EnabledRecurrenceContainer.Dom.appendChild(DaysOfTheWeekContainer.Dom);
    //EnabledRecurrenceContainer.Dom.appendChild(DayPreferenceContainer.Dom)
    EnabledRecurrenceContainer.Dom.appendChild(RepetitionRangeCOntainer.Dom);

    RecurrenceTabContent.Dom.appendChild(EnabledRecurrenceContainer.Dom)
    RecurrenceTabContent.Dom.appendChild(RecurrenceSectionCompletion.Dom);
    createDomEnablingFunction(AllDoms[3], 3, RecurrenceTabContent, DaysOfTheWeekContainer, DaysOfTheWeek.RevealDayOfWeek)();//defaults call to yearly


    


    



    
    function LaunchTab(MiscData) {
        //var CurrentRange = MiscData[0].Content.getCurrentRangeOfAutoContainer();
        var i = 0;
        var NumbeOfValids = new Array();
        for (i; i < AllDoms.length; i++) {
            //if (CurrentRange < AllDoms[i].Range)
            {
                RecurrenceButtonContainer.Dom.appendChild(AllDoms[i].Dom)
                NumbeOfValids.push(AllDoms[i]);
            }
        }

        var PercentageWidth = parseInt(100 / NumbeOfValids.length);

        for (i = 0; i < NumbeOfValids.length; i++)//takes advantage of order of AllDOms. Its arranged from smallest range to bigger
        {
            /*NumbeOfValids[i].Dom.style.width = (PercentageWidth - 1) + "%";
            NumbeOfValids[i].Dom.style.left = (i * PercentageWidth) + "%";*/
        }

        /*$(RecurrenceTabContent.Dom).addClass(CurrentTheme.ActiveTabContent);
        $(RecurrenceTabTitle.Dom).removeClass(CurrentTheme.AlternateFontColor);
        $(RecurrenceTabTitle.Dom).addClass(CurrentTheme.FontColor);
        $(RecurrenceTabTitle.Dom).addClass(CurrentTheme.ContentSection);

        $(RecurrenceTabTitle.Dom).removeClass(CurrentTheme.InActiveTabTitle);
        $(RecurrenceTabTitle.Dom).addClass(CurrentTheme.ActiveTabTitle);*/
    }
    
    LaunchTab();

    var retValue =
    {
        Content: RecurrenceTabContent, Completion: RecurrenceSectionCompletion, RepetitionStatus: EventrepeatStatus, RepetitionSelection: EventRepetitionSelection, RepetitionStart: EventRepeatStart, RepeatEnd: EventRepeatEnd
    };

    return retValue;
}


function generateDayOfWeekRepetitionDom(CallBack) {
    var i = 0;
    var AllDoms = new Array();
    for (i = 0; i < WeekDays.length; i++) {
        AllDoms.push(createDoms(WeekDays[i], i));
    }

    var retValue = { AllDoms: AllDoms, RevealDayOfWeek: createRevealFunction(AllDoms) };

    function createDoms(DayOfWeek, index) {
        var DayDomID = DayOfWeek + "Recurrence" + generateDayOfWeekRepetitionDom.ID++;
        var DayDom = getDomOrCreateNew(DayDomID, "span");

        //DayDom.Dom.innerHTML = "<p class=\"DayOfWeekLetter\">" + DayOfWeek[0] + "</p>";
        DayDom.Dom.innerHTML = DayOfWeek[0];
        DayDom.DayOfWeekIndex = index;
        DayDom.Dom.style.left = ((index * 14.29) + (14.29 / 2)) + "%";
        DayDom.setAttribute('tabindex', 0)
        DayDom.onkeypress = keyEntry
        $(DayDom.Dom).addClass("DayofWeekCircle");
        DayDom.status = 0;
        $(DayDom.Dom).click(generateFunctionForSelectedDayofWeek(DayDom));
        generateFunctionForSelectedDayofWeek(DayDom);
        function generateFunctionForSelectedDayofWeek(DayDom) {

            /*
            DayDom.onfocus=function()
            {
                $(DayDom.Dom).addClass("DayofWeekCircle:hover");
            }
            DayDom.onblur =function()
            {
                $(DayDom.Dom).removeClass("DayofWeekCircle:hover");
            }
            */

            function TurnOffButton ()
            {
                DayDom.status = 0;
                $(DayDom.Dom).removeClass("SelectedDayOfWeek");
                $(DayDom.Dom).addClass("deSelectedDayOfWeek");
                if (CallBack) {
                    CallBack(index, DayDom.status);
                }
            }

            function TurnOnButton() {
                DayDom.status = 1;
                $(DayDom.Dom).removeClass("deSelectedDayOfWeek");
                $(DayDom.Dom).addClass("SelectedDayOfWeek");
                if (CallBack) {
                    CallBack(index, DayDom.status);
                }
            }

            DayDom.TurnOnButton=TurnOnButton;
            DayDom.TurnOffButton = TurnOffButton;

            function clickTrigger ()
            {
                DayDom.status += 1;
                DayDom.status %= 2;
                switch (DayDom.status) {
                    case 0:
                        {
                            TurnOffButton();
                        }
                        break;
                    case 1:
                        {
                            TurnOnButton();
                        }
                        break;
                }
            }

            return clickTrigger
        }


        function keyEntry(e) {
            if (e.which == 32) {
                $(e.target).trigger("click");
                return;
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

generateDayOfWeekRepetitionDom.ID = 0;