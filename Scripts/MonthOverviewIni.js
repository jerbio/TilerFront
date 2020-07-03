"use strict"

var global_WeekGrid;
var global_DayHeight;
var global_WeekWidth;
var global_DayTop;
var global_RangeMultiplier = 5;//range for number of weeks to be specified for calculation
var global_CurrentRange;
var global_ClearRefreshDataInterval = 10000;
var global_ColorAugmentation = 0;
var refreshCounter = 1000;
var global_refreshDataInterval = 45000;
var global_multiSelect;
var global_ControlPanelIconSet = new IconSet();
var global_GoToDay;
var global_eventIsPaused = false;
var global_pauseManager;
var global_CurrentWeekArrangedData = [];
let weeklyScheduleLoadingBar = "weeklyScheduleLoadingBar";
var global_UISetup = { Init: function () { }, RenderOnSubEventClick: null, RenderSubEvent: null, RenderTimeInformation: null, ConflictCalculation: null, ClearUIEffects: function () { },DisplayFullGrid:false,  ButtonID: "", currentSubEvent: null, nextSubEvent: null }
$(global_ControlPanelIconSet.getIconSetContainer()).addClass("ControlPanelIconSetContainer");

var ClassicUIOptions = { Init: function () { }, RenderOnSubEventClick: renderClassicSubEventsClickEvents, RenderSubEvent: renderClassicSubEventLook, RenderTimeInformation: RenderTimeInformationClassic, ConflictCalculation: DoSideBarsConflictClassic, ClearUIEffects: ResetClassicUIEffects,DisplayFullGrid:true, ButtonID: "ClassicViewButton" }
var ListUIOptions = { Init: InitializeListUIEffects, RenderOnSubEventClick: renderSubEventsClickEvents, RenderSubEvent: renderSideBarEvents, RenderTimeInformation: RenderListTimeInformation, ConflictCalculation: DoSideBarsInterSect, ClearUIEffects: ResetListUIEffects,DisplayFullGrid:false, ButtonID: "ListViewButton" }

var AllUIOptions = [ListUIOptions, ClassicUIOptions];
let PreviewtDataDict = {}

$(document).ready(function () {
    LaumchUIOPtion(0);
    $(document).tooltip({ track: true });
    $('body').hide();
    global_pauseManager = new GlobaPauseResumeButtonManager([]);
    getRefreshedData.pauseEnroll(global_pauseManager.processPauseData);
    initializeWebSockets();
    InitializeMonthlyOverview();
    MenuManger();
    
});


function LaumchUIOPtion(UIOptionIndex)
{
    LaumchUIOPtion.CurrentIndex = UIOptionIndex % AllUIOptions.length;
    UpdateUISetup(AllUIOptions[LaumchUIOPtion.CurrentIndex]);
    var NextIndex = (LaumchUIOPtion.CurrentIndex + 1) % AllUIOptions.length;
    var NextViewButtonID = AllUIOptions[NextIndex].ButtonID;
    var NextViewButton = getDomOrCreateNew(NextViewButtonID);
    var ViewContainer = getDomOrCreateNew("ViewContainer");
    var CurrentActiveButton = ViewContainer.firstElementChild;
    CurrentActiveButton.parentNode.insertBefore(NextViewButton, CurrentActiveButton);
    function generateNextCall()
    {
        return function()
        {
            LaumchUIOPtion(NextIndex);
        }
    }

    NextViewButton.onclick = generateNextCall();
}

LaumchUIOPtion.CurrentIndex = 0;

function UpdateUISetup(UIOptions)
{
    global_ExitManager.triggerLastExitAndPop();
    global_UISetup.ClearUIEffects();
    global_UISetup.Init = UIOptions.Init;
    global_UISetup.Init();
    global_UISetup.RenderSubEvent = UIOptions.RenderSubEvent;
    global_UISetup.RenderOnSubEventClick = UIOptions.RenderOnSubEventClick;
    global_UISetup.ConflictCalculation = UIOptions.ConflictCalculation;
    global_UISetup.RenderTimeInformation = UIOptions.RenderTimeInformation;
    global_UISetup.ClearUIEffects = UIOptions.ClearUIEffects;
    global_UISetup.DisplayFullGrid = UIOptions.DisplayFullGrid;
    if (global_UISetup.DisplayFullGrid)
    {
        setTimeout(DisplayFullGrid, 100)///using timeout in case grid has not been generated yet, this occurs with the initializing call to LaumchUIOPtion, which is triggered before the rendering of the week schedule
    }
    else
    {
        setTimeout(HideFullGrid, 100)///using timeout in case grid has not been generated yet
        //HideFullGrid();
    }
    setTimeout(function () { TriggerWeekUIupdate(global_CurrentWeekArrangedData); }, 1000);
}

function HideFullGrid()
{
    var AllFullGridContainer = $(".FullGridContainer");
    if (AllFullGridContainer.length)
        for (var ID = 0 ; ID < AllFullGridContainer.length; ID++)
        {
            AllFullGridContainer[ID].isEnabled = false;
            $(AllFullGridContainer[ID]).addClass("setAsDisplayNone");
        }

    var DayOfTime = $(".DayOfTime");
    if (DayOfTime.length)
        for (var ID = 0 ; ID < DayOfTime.length; ID++) {
            $(DayOfTime[ID]).addClass("setAsDisplayNone");
        }
    var VerticalScrollContainer = getDomOrCreateNew("VerticalScrollContainer");
    $(VerticalScrollContainer).removeClass("ZoomInto12Hours");
    $(VerticalScrollContainer).addClass("ZoomInto24Hours");
    $(VerticalScrollContainer).removeClass("FullGridEnhancements");
    var NameOfWeekContainerPlane = getDomOrCreateNew("NameOfWeekContainerPlane");
    $(NameOfWeekContainerPlane).removeClass("FullGridEnhancements");
    var LeftContainer = getDomOrCreateNew("leftDayOfTime");
    $(LeftContainer).removeClass("ZoomInto12Hours");
    $(LeftContainer).addClass("ZoomInto24Hours");
    
}

function DisplayFullGrid()
{
    var AllFullGridContainer = $(".FullGridContainer")
    if (AllFullGridContainer.length)
        for (var ID = 0 ; ID < AllFullGridContainer.length; ID++)
        {
            AllFullGridContainer[ID].isEnabled = true;
            $(AllFullGridContainer[ID]).removeClass("setAsDisplayNone");
        }

    var DayOfTime = $(".DayOfTime");
    if (DayOfTime.length)
        for (var ID = 0 ; ID < DayOfTime.length; ID++) {
            $(DayOfTime[ID]).removeClass("setAsDisplayNone");
        }
    

    var VerticalScrollContainer = getDomOrCreateNew("VerticalScrollContainer");
    $(VerticalScrollContainer).removeClass("ZoomInto24Hours");
    $(VerticalScrollContainer).addClass("ZoomInto12Hours");
    $(VerticalScrollContainer).addClass("FullGridEnhancements");
    var NameOfWeekContainerPlane = getDomOrCreateNew("NameOfWeekContainerPlane");
    $(NameOfWeekContainerPlane).addClass("FullGridEnhancements");
    var LeftContainer = getDomOrCreateNew("leftDayOfTime");
    $(LeftContainer).removeClass("ZoomInto24Hours");
    $(LeftContainer).addClass("ZoomInto12Hours");
}

function MenuManger()
{
    var TilerManager = getDomOrCreateNew("TilerManager");
    TilerManager.RevealCount=0;
    TilerManager.onmouseover = revealMenuContainer;
    TilerManager.onmouseout = unRevealMenuContainer;
    var MenuContainer = getDomOrCreateNew("MenuContainer");


    function revealMenuContainer()
    {
        //setTimeout(
        //function()
        {
            MenuContainer.style.display = "inline-block";
            $(MenuContainer).removeClass("setAsDisplayNone");
            MenuContainer.style.width = "auto";
            MenuContainer.style.left = "-20px"
            ++TilerManager.RevealCount;
        }//,200)
    }

    function unRevealMenuContainer()
    {
        function getCurrentCount()
        {
            var RetValue = TilerManager.RevealCount
            return RetValue;
        }
        var CurrentCount = getCurrentCount();
        ResolveHideStatus(CurrentCount);


        function ResolveHideStatus(CountSofar)
        {
            setTimeout(function () {
                if (CountSofar==getCurrentCount())
                {
                    TriggerHide();
                    TilerManager.RevealCount=0
                    return;
                }
                return;
            },50)
        }

        function TriggerHide()
        {
            MenuContainer.style.display = "none";
            MenuContainer.style.width = 0;
            MenuContainer.style.left = "-50px"
        }
        

    }

    var ManageMenuContainer = getDomOrCreateNew("ManageMenuContainer");
    ManageMenuContainer.onclick = function () {
        var manageURL ="/Manage"
        window.location.href = manageURL;
    }

    var LogoutMenuContainer = getDomOrCreateNew("LogoutMenuContainer");
    LogoutMenuContainer.onclick = function () {
        //var logoutForm = getDomOrCreateNew("logoutForm");
        document.forms["logoutForm"].submit();
    }
}

function RenderSleep() {
    if(global_CurrentWeekArrangedData && global_CurrentWeekArrangedData.length > 0 && global_sleepTimeline) {
        global_CurrentWeekArrangedData.forEach((weekRange) => {
            weekRange.DaysOfWeek.forEach((day) => {
                day.RenderSleepSection(day.Start, global_sleepTimeline);
            });
        });
    }
}

function RenderTimeInformationClassic(DayOfWeek, ID) {
    var RefSubEvent = global_DictionaryOfSubEvents[ID];
    var TopPixels = ((DayOfWeek.UISpecs[ID].css.top / 100) * global_DayHeight) + global_DayTop;

    var ListElementContainer = getDomOrCreateNew("SubEventReference" + ID);
    ListElementContainer.setAttribute("draggable", true);
    ListElementContainer.ondragstart = OnDragStartOfSubEvent;
    $(ListElementContainer.Dom).addClass("TimeDataFormat");
    
    RefSubEvent.gridDoms.push(ListElementContainer.Dom)//Adds the List element as list of candidates to be deleted
    
    DayOfWeek.UISpecs[ID].refrenceListElement = ListElementContainer;

    var TimeDataPerListElement = getDomOrCreateNew("SubEventReferenceTime" + ID);
    var BottomPanelListElement = getDomOrCreateNew("SubEventReferenceBottomPanel" + ID);
    $(BottomPanelListElement.Dom).addClass("SubEventReferenceBottomPanel");

    var CalendarTypeContainer = getDomOrCreateNew("SubEventReferenceCalType" + ID);
    var CalendarTypeCalImage = getDomOrCreateNew("SubEventReferenceCalImg" + ID);

    $(CalendarTypeCalImage.Dom).addClass("SubEventReferenceCalImg");
    $(CalendarTypeCalImage.Dom).addClass(RefSubEvent.ThirdPartyType + "Icon");
    $(CalendarTypeContainer.Dom).addClass("SubEventReferenceCalType");

    CalendarTypeContainer.appendChild(CalendarTypeCalImage);

    var NameDataPerListElement = getDomOrCreateNew("SubEventReferenceName" + ID);
    var EventLockContainer = getDomOrCreateNew("EventLockContainer" + ID);
    //BottomPanelListElement.appendChild(CalendarTypeContainer);
    EventLockContainer.appendChild(CalendarTypeContainer);
    BottomPanelListElement.appendChild(TimeDataPerListElement);

    
    //$(obj).addClass("ListElements");
    NameDataPerListElement.Dom.innerHTML = RefSubEvent.Name;
    TimeDataPerListElement.Dom.innerHTML = getTimeStringFromDate(RefSubEvent.SubCalStartDate) + " - " + getTimeStringFromDate(RefSubEvent.SubCalEndDate);
    $(NameDataPerListElement.Dom).addClass("SubEventReferenceName");
    $(TimeDataPerListElement.Dom).addClass("SubEventReferenceTime");




    var ListElementDataContentContainer = getDomOrCreateNew("ListElementDataContentContainer" + ID);
    $(ListElementDataContentContainer.Dom).addClass("ListElementDataContentContainerClassic");
    ListElementDataContentContainer.Dom.appendChild(NameDataPerListElement.Dom);
    ListElementDataContentContainer.Dom.appendChild(BottomPanelListElement.Dom);

    


    var myBool = (RefSubEvent.SubCalRigid)
    $(EventLockContainer.Dom).addClass("EventLockContainer");
    ListElementContainer.Dom.appendChild(EventLockContainer.Dom);
    if (myBool)
    {
        var EventLockImgContainer = getDomOrCreateNew("EventLockImgContainer" + ID);
        $(EventLockImgContainer.Dom).addClass("EventLockImgContainer");
        EventLockContainer.Dom.appendChild(EventLockImgContainer.Dom);
        
        $(EventLockImgContainer.Dom).addClass("LockedIcon");
    }

    let EventDeadlineColorContainer = getDomOrCreateNew("EventDeadlineColorContainer" + ID);
    $(EventDeadlineColorContainer.Dom).addClass("EventDeadlineColorContainer");
    let EventDeadlineColorImage = getDomOrCreateNew("EventDeadlineColorImage" + ID);
    EventDeadlineColorContainer.Dom.appendChild(EventDeadlineColorImage.Dom);
    let refTime = (RefSubEvent.SubCalCalEventEnd).getTime()
    var pastDeadlineColor = Date.now() > refTime
    $(EventDeadlineColorImage.Dom).removeClass("PastDeadline OneDay ThreeDay MoreThanThreeDay");
    let span = refTime - Date.now()
    if (span < 0) {
        $(EventDeadlineColorImage.Dom).addClass("PastDeadline EventDeadlineColorImage");
    } else if (span < OneDayInMs) {
        $(EventDeadlineColorImage.Dom).addClass("OneDay EventDeadlineColorImage");
    } else if (span < (OneDayInMs * 3)) {
        $(EventDeadlineColorImage.Dom).addClass("ThreeDay EventDeadlineColorImage");
    } else {
        $(EventDeadlineColorImage.Dom).addClass("MoreThanThreeDay EventDeadlineColorImage");
    }
    EventLockContainer.Dom.appendChild(EventDeadlineColorContainer.Dom);


    ListElementContainer.Dom.appendChild(ListElementDataContentContainer.Dom);
    

    //RefSubEvent.TimeSizeDom.appendChild(ListElementDataContentContainer);
    //DayOfWeek.UISpecs[ID].DataElement = ListElementDataContentContainer
    RefSubEvent.TimeSizeDom.appendChild(ListElementContainer);

    var HeightPx = (DayOfWeek.UISpecs[ID].css.height / 100) * global_DayHeight;
    HeightPx = HeightPx < 50 ? 50 : HeightPx;
    var EndPixelTop = TopPixels + HeightPx;
    ///BestBottom is data on tab a level which ends before myData. BestBottom.Count data member is the level, BestBOttom.End is the end pixel of this base tab
    var RetValue = { Start: DayOfWeek.UISpecs[ID].Start, CalCCount: 0, Data: DayOfWeek.UISpecs[ID], ID: ID, BestBottom: { End: 10000, Count: 0 }, Count: 0, EarlierCount: 0, top: TopPixels, end: EndPixelTop, PrecedingOverlapers: 0, OverlappingCount: 0,OverlappingAfterMe:0 }
    return RetValue;
}




/*Function clears out the pertinent UI effects of the Classic UI*/
function ResetClassicUIEffects()
{
    //debugger;
    getRefreshedData.disableDataRefresh();
    TotalSubEventList.forEach(ResetSubEvent);
    for (var ID in global_DictionaryOfSubEvents) {
        global_DictionaryOfSubEvents[ID].Bind = null;
    }
    function ResetSubEvent(SubEvent)
    {
        var AllNodesWithListElementDataContentContainerClassic = $(SubEvent.TimeSizeDom).find(".ListElementDataContentContainerClassic");
        if (AllNodesWithListElementDataContentContainerClassic.length)
        {
            //debugger;
            $(AllNodesWithListElementDataContentContainerClassic).removeClass("ListElementDataContentContainerClassic");
        }

        var AllNodesWithTimeDataFormat = $(SubEvent.TimeSizeDom).find(".TimeDataFormat");
        if (AllNodesWithTimeDataFormat.length) {
            $(AllNodesWithTimeDataFormat).removeClass("TimeDataFormat");
        }


        $(SubEvent.TimeSizeDom).removeClass("ClassicGridEvents");
    }

    var AllClassicGridEvents = $(".ClassicGridEvents");
    if (AllClassicGridEvents.length) {
        for (var i = 0 ; i < AllClassicGridEvents.length; i++) {
            var ClassicGridEvents = AllClassicGridEvents[i];
            $(ClassicGridEvents).removeClass("ClassicGridEvents");
        }
    }

    getRefreshedData.enableDataRefresh();
}

function RenderListTimeInformation(DayOfWeek, ID, isNext)
{
    var RefSubEvent = global_DictionaryOfSubEvents[ID];
    let now = Date.now()
    var TopPixels = ((DayOfWeek.UISpecs[ID].css.top / 100) * global_DayHeight) + global_DayTop;
    var ListElementContainer = getDomOrCreateNew("SubEventReference" + ID);
    ListElementContainer.setAttribute("draggable", true);
    ListElementContainer.ondragstart = OnDragStartOfSubEvent;
    //ListElementContainer.ondrop = OnDropOfSubEvent;
    //ListElementContainer.ondragstart = OnDragStartOfSubEvent;
    //ListElementContainer.ondragstart = OnDragStartOfSubEvent;
    $(ListElementContainer.Dom).addClass("TimeDataFormat");
    $(ListElementContainer.Dom).addClass("ListElement");
    
    RefSubEvent.gridDoms.push(ListElementContainer.Dom)//Adds the List element as list of candidates to be deleted
    
    DayOfWeek.UISpecs[ID].refrenceListElement = ListElementContainer;
    var TimeDataPerListElement = getDomOrCreateNew("SubEventReferenceTime" + ID);
    var BottomPanelListElement = getDomOrCreateNew("SubEventReferenceBottomPanel" + ID);
    $(BottomPanelListElement.Dom).addClass("SubEventReferenceBottomPanel");

    var CalendarTypeContainer = getDomOrCreateNew("SubEventReferenceCalType" + ID);
    var CalendarTypeCalImage = getDomOrCreateNew("SubEventReferenceCalImg" + ID);

    $(CalendarTypeCalImage.Dom).addClass("SubEventReferenceCalImg");
    $(CalendarTypeCalImage.Dom).addClass(RefSubEvent.ThirdPartyType + "Icon");
    $(CalendarTypeContainer.Dom).addClass("SubEventReferenceCalType");

    var SubEventTimeTillDeadlineContainer = getDomOrCreateNew("SubEventTimeTillDeadlineContainer" + ID);
    $(SubEventTimeTillDeadlineContainer).addClass("SubEventTimeTillDeadlineContainer");
    var SubEventTimeTillDeadlineContent = getDomOrCreateNew("SubEventTimeTillDeadlineContent" + ID, "span");
    $(SubEventTimeTillDeadlineContent).addClass("SubEventTimeTillDeadlineContent");
    SubEventTimeTillDeadlineContent.innerHTML = moment(RefSubEvent.SubCalCalEventEnd).fromNow()
    SubEventTimeTillDeadlineContainer.appendChild(SubEventTimeTillDeadlineContent);
    BottomPanelListElement.appendChild(SubEventTimeTillDeadlineContainer);

    CalendarTypeContainer.appendChild(CalendarTypeCalImage);

    var NameDataPerListElement = getDomOrCreateNew("SubEventReferenceName" + ID);


    
    //BottomPanelListElement.appendChild(CalendarTypeContainer);

    //debugger

    //BottomPanelListElement.appendChild(CalendarTypeContainer);
    BottomPanelListElement.appendChild(TimeDataPerListElement);

    $(ListElementContainer.Dom).addClass("selectedDayElements");
    // $(ListElementContainer.Dom).removeClass("ListElements");
    //$(obj).addClass("ListElements");
    NameDataPerListElement.Dom.innerHTML = RefSubEvent.Name;
    TimeDataPerListElement.Dom.innerHTML = getTimeStringFromDate(RefSubEvent.SubCalStartDate) + " - " + getTimeStringFromDate(RefSubEvent.SubCalEndDate);
    $(NameDataPerListElement.Dom).addClass("SubEventReferenceName");
    $(TimeDataPerListElement.Dom).addClass("SubEventReferenceTime");


    var ColorContainer = getDomOrCreateNew("ListElementDataContentContainerColorContainer" + ID);
    $(ColorContainer).addClass("ListElementColorContainer");

    var ListElementDataContentContainer = getDomOrCreateNew("ListElementDataContentContainer" + ID);
    $(ListElementDataContentContainer.Dom).addClass("ListElementDataContentContainer");
    ListElementDataContentContainer.Dom.appendChild(NameDataPerListElement.Dom);
    ListElementDataContentContainer.Dom.appendChild(BottomPanelListElement.Dom);

    RefSubEvent.gridDoms.push(ListElementDataContentContainer.Dom)
    ColorContainer.appendChild(ListElementDataContentContainer)

    var EventLockContainer = getDomOrCreateNew("EventLockContainer" + ID);
    $(EventLockContainer.Dom).addClass("EventLockContainer");
    var myBool = (RefSubEvent.SubCalRigid)
    var EventLockImgContainer = getDomOrCreateNew("EventLockImgContainer" + ID);
    $(EventLockImgContainer.Dom).addClass("EventLockImgContainer");
    if (myBool) {
        $(EventLockImgContainer.Dom).addClass("LockedIcon");
    }
    
    let EventDeadlineColorContainer = getDomOrCreateNew("EventDeadlineColorContainer" + ID);
    $(EventDeadlineColorContainer.Dom).addClass("EventDeadlineColorContainer");
    let EventDeadlineColorImage = getDomOrCreateNew("EventDeadlineColorImage" + ID);
    $(EventDeadlineColorImage.Dom).removeClass("PastDeadline OneDay ThreeDay MoreThanThreeDay");
    EventDeadlineColorContainer.Dom.appendChild(EventDeadlineColorImage.Dom);
    let refTime = (RefSubEvent.SubCalCalEventEnd).getTime()
    var pastDeadlineColor = Date.now() > refTime
    let span = refTime - Date.now() 
    if (span < 0) {
        $(EventDeadlineColorImage.Dom).addClass("PastDeadline EventDeadlineColorImage");
    } else if (span < OneDayInMs) {
        $(EventDeadlineColorImage.Dom).addClass("OneDay EventDeadlineColorImage");
    } else if (span < (OneDayInMs * 3)) {
        $(EventDeadlineColorImage.Dom).addClass("ThreeDay EventDeadlineColorImage");
    } else {
        $(EventDeadlineColorImage.Dom).addClass("MoreThanThreeDay EventDeadlineColorImage");
    }

    EventLockContainer.Dom.appendChild(EventLockImgContainer.Dom)
    EventLockContainer.Dom.appendChild(CalendarTypeContainer.Dom);
    EventLockContainer.Dom.appendChild(EventDeadlineColorContainer.Dom);


    ListElementContainer.Dom.appendChild(EventLockContainer.Dom);
    ListElementContainer.Dom.appendChild(ColorContainer.Dom);

    
    DayOfWeek.renderPlane.Dom.appendChild(ListElementContainer);
    //DayOfWeek.renderPlane.Dom.appendChild(DayOfWeek.UISpecs[ID].refrenceListElement.Dom)

    DayOfWeek.UISpecs[ID].DataElement = ColorContainer;
    RefSubEvent.ListRefElement = ListElementContainer;

    let isCurrentDayOfWeek = now < DayOfWeek.End.getTime() && now >= DayOfWeek.Start.getTime();
    if(isCurrentDayOfWeek) {
        let isCurrentSubEvent = now < RefSubEvent.SubCalEndDate.getTime() && now >= RefSubEvent.SubCalStartDate.getTime() && !RefSubEvent.isAllDay;
        if(isCurrentSubEvent) {
            global_UISetup.currentSubEvent = RefSubEvent;
            renderNowUi(RefSubEvent);
        }
        if(!global_UISetup.currentSubEvent && !global_UISetup.nextSubEvent) {
            let isNext = now < RefSubEvent.SubCalStartDate.getTime();
            if(isNext) {
                global_UISetup.nextSubEvent = RefSubEvent;
                renderNextUi(RefSubEvent);
            }
        }
    }

    var HeightPx = (DayOfWeek.UISpecs[ID].css.height / 100) * global_DayHeight;
    var EndPixelTop = TopPixels + HeightPx;
    ///BestBottom is data on tab a level which ends before myData. BestBottom.Count data member is the level, BestBOttom.End is the end pixel of this base tab
    var RetValue= { 
        Start: DayOfWeek.UISpecs[ID].Start, 
        CalCCount: 0, 
        Data: DayOfWeek.UISpecs[ID], 
        ID: ID, 
        BestBottom: { End: 10000, Count: 0 }, 
        Count: 0, 
        EarlierCount: 0, 
        top: TopPixels, 
        end: EndPixelTop,
        refSubEvent: ListElementContainer };
    return RetValue;

}

function InitializeListUIEffects()
{
    var AllSubEventListCOntainer = $(".SubEventListContainer");
    if (AllSubEventListCOntainer.length) {
        for (var i = 0 ; i < AllSubEventListCOntainer.length; i++) {
            var DayOfWeekListCOntainer = AllSubEventListCOntainer[i];
            $(DayOfWeekListCOntainer).removeClass("setAsDisplayNone");
        }
    }
}

function ResetListUIEffects()
{
    getRefreshedData.disableDataRefresh();
    TotalSubEventList.forEach(ResetSubEvent);

    for (var ID in global_DictionaryOfSubEvents)
    {
        global_DictionaryOfSubEvents[ID].Bind=null;
    }

    var AllListElementColorContainer = $(".ListElementColorContainer");
    if (AllListElementColorContainer.length)
    {
        for (var i = 0 ; i < AllListElementColorContainer.length; i++)
        {
            var DayOfWeekListCOntainer = AllListElementColorContainer[i];
            DayOfWeekListCOntainer.parentNode.removeChild(DayOfWeekListCOntainer);
            //$(DayOfWeekListCOntainer).removeClass("ListElementColorContainer");
        }
    }

    var AllSubEventListCOntainer = $(".SubEventListContainer");
    if (AllSubEventListCOntainer.length)
    {
        for (var i = 0 ; i < AllSubEventListCOntainer.length; i++)
        {
            var DayOfWeekListCOntainer = AllSubEventListCOntainer[i];
            $(DayOfWeekListCOntainer).addClass("setAsDisplayNone");
        }
    }

    var SideBarElements = $(".SideBar");
    if (SideBarElements.length) {
        for (var i = 0 ; i < SideBarElements.length; i++) {
            var SideBar = SideBarElements[i];
            $(SideBar).removeClass("SideBar");
        }
    }



    //$(SubEvent.TimeSizeDom).removeClass("SideBar");

    function ResetSubEvent(SubEvent)
    {
        var ListElementDataContentContainer = $(SubEvent.ListRefElement).find(".ListElementDataContentContainer");
        if (ListElementDataContentContainer.length) {
            //debugger;
            $(ListElementDataContentContainer).removeClass("ListElementDataContentContainer");
        }

        var AllNodesWithListElement = $(SubEvent.TimeSizeDom).find(".ListElement");
        if (AllNodesWithListElement.length) {
            $(AllNodesWithListElement).removeClass("ListElement");
        }
    }
    getRefreshedData.enableDataRefresh();
}
/*
Add Close button to the given element. Container is Dom element to which you want to add a close button. if you want to the close button to be on the top left set LeftPosition to true
*/
function AddCloseButoon(Container, LeftPosition)
{
    let counterId = AddCloseButoon.getId();
    var TilerCloseButtonID = "TilerCloseButton"+counterId;
    var TilerCloseButton = getDomOrCreateNew(TilerCloseButtonID);
    $(TilerCloseButton.Dom).attr('tabindex', 0)
    $(TilerCloseButton).removeClass("LeftCloseButton");
    $(TilerCloseButton).removeClass("RightCloseButton");
    $(TilerCloseButton).removeClass("setAsDisplayNone");
    $(TilerCloseButton).addClass('TilerCloseButton');

    if(LeftPosition)
    {
        $(TilerCloseButton).addClass("LeftCloseButton");
    }
    else
    {
        $(TilerCloseButton).addClass("RightCloseButton");
    }
    
    let leftClosebarId ="LeftCloseBar"+counterId;
    let rightClosebarId ="RightCloseBar"+counterId;
    let LeftCloseBar = getDomOrCreateNew(leftClosebarId);
    let RightCloseBar = getDomOrCreateNew(rightClosebarId);
    
    $(LeftCloseBar).addClass("LeftCloseBar");
    $(RightCloseBar).addClass("RightCloseBar");

    TilerCloseButton.onclick = function (e) {
        e.stopPropagation();
        global_ExitManager.triggerLastExitAndPop();
    };
    TilerCloseButton.appendChild(LeftCloseBar);
    TilerCloseButton.appendChild(RightCloseBar);
    

    Container.appendChild(TilerCloseButton);
    return TilerCloseButton;
}

AddCloseButoon.HideClosButton = function()
{
    var TilerCloseButton = getDomOrCreateNew("TilerCloseButton");
    $(TilerCloseButton).addClass("setAsDisplayNone");
};


AddCloseButoon.CloseCounter = 0;
AddCloseButoon.getId = () => {
    AddCloseButoon.CloseCounter+=1;
    return AddCloseButoon.CloseCounter;
}
function RevealControlPanelSection(SelectedEvents)
{
    //global_ExitManager.triggerLastExitAndPop();
    getRefreshedData.disableDataRefresh();
    var yeaButton = getDomOrCreateNew("YeaToConfirmDelete");
    var nayButton = getDomOrCreateNew("NayToConfirmDelete");
    var completeButton = RevealControlPanelSection.IconSet.getCompleteButton();
    var deleteButton = RevealControlPanelSection.IconSet.getDeleteButton();
    var DeleteMessage = getDomOrCreateNew("DeleteMessage")
    var ProcastinationButton = getDomOrCreateNew("submitProcastination");
    var ProcastinationCancelButton = getDomOrCreateNew("cancelProcastination");
    var PreviewButton = getDomOrCreateNew("previewProcastination");
    var ControlPanelCloseButton = RevealControlPanelSection.IconSet.getCloseButton();
    $(ControlPanelCloseButton).removeClass("setAsDisplayNone")
    var ProcrastinateEventModalContainer = getDomOrCreateNew("ProcrastinateEventModal");
    var ControlPanelProcrastinateButton = RevealControlPanelSection.IconSet.getProcrastinateButton();
    RevealControlPanelSection.IconSet.hideProcrastinateButton();
    RevealControlPanelSection.IconSet.hideRepeatButton();
    $(RevealControlPanelSection.IconSet.getLocationButton()).addClass("setAsDisplayNone");
    var ModalDelete = getDomOrCreateNew("ConfirmDeleteModal")
    var MultiSelectPanel = getDomOrCreateNew("MultiSelectPanel")
    var ControlPanelContainer = getDomOrCreateNew("ControlPanelContainer");
    var IconSetContainer = RevealControlPanelSection.IconSet.getIconSetContainer();
    var PauseResumeButton = RevealControlPanelSection.IconSet.getPauseResumeButton();
    $(PauseResumeButton).addClass("setAsDisplayNone")
    $(ControlPanelContainer).addClass("ControlPanelContainerLowerBar");
    if (Object.keys(SelectedEvents).length < 1) {
        $(MultiSelectPanel).addClass("hideMultiSelectPanel");
        global_ExitManager.triggerLastExitAndPop();
        return;
    }

    ControlPanelContainer.style.left = "auto";
    ControlPanelContainer.style.top = "auto";


    addLowerBarIconSetCOntainer();

    hideControlInfoContainer();
    $(MultiSelectPanel).removeClass("hideMultiSelectPanel");
    global_UISetup.RenderOnSubEventClick.BottomPanelIsOpen = true;
    ControlPanelContainer.appendChild(IconSetContainer);

    $(global_ControlPanelIconSet.getIconSetContainer())

    $('#ControlPanelContainer').slideDown(500);

    Object.keys(PreviewtDataDict).forEach((key) => {
        let preview = PreviewtDataDict[key];
        preview.hide();
    })

    function resetButtons() {
        yeaButton.onclick = null;
        nayButton.onclick = null;
        ControlPanelProcrastinateButton.onclick = null;
        ControlPanelCloseButton.onclick = null;
    }

    function closeModalDelete() {
        $('#ConfirmDeleteModal').slideUp(500);
        ModalDelete.isRevealed = false;
    }

    function closeProcrastinatePanel() {
        $(ProcrastinateEventModalContainer).slideUp(500);
    }
    
    

    function closeControlPanel()
    {
        TriggerClose();
    }

    function TriggerClose ()
    {
        getRefreshedData.enableDataRefresh();
        resetButtons();
        closeModalDelete();
        closeProcrastinatePanel();
        deleteButton.onclick = null;
        completeButton.onclick = null;
        $('#ControlPanelContainer').slideUp(500);
        $(MultiSelectPanel).addClass("hideMultiSelectPanel");
        global_UISetup.RenderOnSubEventClick.BottomPanelIsOpen = false;
        document.removeEventListener("keydown", containerKeyPress);
        $(ControlPanelProcrastinateButton).removeClass("setAsDisplayNone");
        $(RevealControlPanelSection.IconSet.getLocationButton()).removeClass("setAsDisplayNone");
        if (IconSetContainer.parentNode!=null)
        {
            IconSetContainer.parentNode.removeChild(IconSetContainer);
        }
        $(ControlPanelContainer).removeClass("ControlPanelContainerLowerBar");
        $(ControlPanelContainer).removeClass("ControlPanelContainerModal");
    }

    RevealControlPanelSection.Exit = closeControlPanel;



    function deleteSubevent()//triggers the yea / nay deletion of events
    {
        //debugger;
        DeleteMessage.innerHTML = "Sure you want to delete " + Object.keys(SelectedEvents).length + " Events?"
        yeaButton.onclick = yeaDeleteSubEvent;
        nayButton.onclick = nayDeleteSubEvent;
        yeaButton.focus();
        $('#ConfirmDeleteModal').slideDown(500);
        ModalDelete.isRevealed = true;
    }

    function containerKeyPress(e) {
        e.stopPropagation();
        if (e.which == 27)//escape key press
        {
            return;
            //closeControlPanel();
        }
        
        if ((e.which == 8) || (e.which == 46))//bkspc/delete key pressed
        {
            deleteSubevent();
        }
    }

    function yeaDeleteSubEvent()//triggers the deletion of subevent
    {
        SendMessage();
        function SendMessage() {
            var TimeZone = new Date().getTimezoneOffset();
            let keyArray = [];
            let thirdPartyUserIdArray = [];
            let ThirdPartyTypeArray = [];
            for(let key in SelectedEvents) {
                keyArray.push(key);
                let subEvent = SelectedEvents[key];
                let userId = subEvent.ThirdPartyUserID;
                let ThirdPartyType = subEvent.ThirdPartyType;
                if(userId === null && ThirdPartyType !== "tiler") {
                    userId = "not_valid_user@mytiler.com";
                }
                thirdPartyUserIdArray.push(userId);
                ThirdPartyTypeArray.push(ThirdPartyType);
            }
            var AllIds = keyArray.join(',');
            let thirdPartyUserIds= thirdPartyUserIdArray.join(',');
            let ThirdPartyType = ThirdPartyTypeArray.join(',');
            

            var DeletionEvent = {
                UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: AllIds, TimeZoneOffset: TimeZone, thirdPartyUserId:thirdPartyUserIds, ThirdPartyType: ThirdPartyType
            };
            //var URL = "RootWagTap/time.top?WagCommand=6"
            var URL = global_refTIlerUrl + "Schedule/Events";
            DeletionEvent.TimeZone = moment.tz.guess()
            preSendRequestWithLocation(DeletionEvent);
            var HandleNEwPage = new LoadingScreenControl("Tiler is Deleting your event :)");
            HandleNEwPage.Launch();

            var exitSendMessage = function (data) {
                HandleNEwPage.Hide();
                //triggerUIUPdate();//hack alert
                global_ExitManager.triggerLastExitAndPop();
                //getRefreshedData();
            }

            $.ajax({
                type: "DELETE",
                url: URL,
                data: DeletionEvent,
                // DO NOT SET CONTENT TYPE to json
                // contentType: "application/json; charset=utf-8", 
                // DataType needs to stay, otherwise the response object
                // will be treated as a single string
                success: function (response) {
                    exitSendMessage()
                    triggerUndoPanel("Undo deletion?");
                },
                error: function () {
                    var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                    var ExitAfter = {
                        ExitNow: true, Delay: 1000
                    };
                    HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exitSendMessage);
                }
            }).done(function (data) {
                HandleNEwPage.Hide();
                triggerUIUPdate();//hack alert
                sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
            });
        }
        function triggerUIUPdate() {
            //alert("we are deleting " + SubEvent.ID);
            //$('#ConfirmDeleteModal').slideToggle();
            //$('#ControlPanelContainer').slideUp(500);
            //resetButtons();
            global_ExitManager.triggerLastExitAndPop();
        }

    }
    function nayDeleteSubEvent()//ignores deletion of events
    {
        closeModalDelete();
        //resetButtons();
    }

    function markAsComplete() {
        SendMessage();
        function SendMessage() {
            var TimeZone = new Date().getTimezoneOffset();
            var Url;
            //Url="RootWagTap/time.top?WagCommand=7";
            Url = global_refTIlerUrl + "Schedule/Events/Complete";
            var HandleNEwPage = new LoadingScreenControl("Tiler is updating your schedule ...");
            HandleNEwPage.Launch();

            let keyArray = [];
            let thirdPartyUserIdArray = [];
            let ThirdPartyTypeArray = [];


            for(let key in SelectedEvents) {
                keyArray.push(key);
                let subEvent = SelectedEvents[key];
                let userId = subEvent.ThirdPartyUserID;
                let ThirdPartyType = subEvent.ThirdPartyType;
                if(userId === null && ThirdPartyType !== "tiler") {
                    userId = "not_valid_user@mytiler.com";
                }
                thirdPartyUserIdArray.push(userId);
                ThirdPartyTypeArray.push(ThirdPartyType);
            }
            var AllIds = keyArray.join(',');
            let thirdPartyUserIds= thirdPartyUserIdArray.join(',');
            let ThirdPartyType = ThirdPartyTypeArray.join(',');

            var MarkAsCompleteData = {
                UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: AllIds, TimeZoneOffset: TimeZone, thirdPartyUserId:thirdPartyUserIds, ThirdPartyType: ThirdPartyType
            };
            MarkAsCompleteData.TimeZone = moment.tz.guess()
            preSendRequestWithLocation(MarkAsCompleteData);
            var exit = function (data) {
                HandleNEwPage.Hide();
                //triggerUIUPdate();//hack alert
                global_ExitManager.triggerLastExitAndPop();
                sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
            }


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
                    triggerUndoPanel("Undo");
                },
                error: function (err) {
                    var myError = err;
                    var step = "err";
                    var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                    var ExitAfter = {
                        ExitNow: true, Delay: 1000
                    };
                    HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exit);
                    //InitializeHomePage();


                }

            }).done(function (data) {
                HandleNEwPage.Hide();
                triggerUIUPdate();//hack alert
                //getRefreshedData();
            });
        }
        function triggerUIUPdate() {
            //resetButtons();
            global_ExitManager.triggerLastExitAndPop();
        }

    }
    completeButton.onclick = markAsComplete;

    document.removeEventListener("keydown", containerKeyPress);//this is here just to avooid duplicate addition of the same keypress event
    document.addEventListener("keydown", containerKeyPress);
   
    MultiSelectPanel.innerHTML = Object.keys(SelectedEvents).length+" Events Selected"
    ControlPanelCloseButton.onclick = global_ExitManager.triggerLastExitAndPop
    deleteButton.onclick = deleteSubevent;
    if (ModalDelete.isRevealed)
    {
        deleteSubevent();
    }
}
RevealControlPanelSection.IconSet = global_ControlPanelIconSet;
RevealControlPanelSection.CallBack = RevealControlPanelSection;

function IconSet()
{
    if (!IconSet.ID) {
        IconSet.ID = 0;
    }
    var myID = IconSet.ID++;
    var IconSetContainerID = "IconSetContainer"+myID
    var IconSetContainer = getDomOrCreateNew(IconSetContainerID);
    //$(IconSetContainer).addClass("ControlPanelIconSetContainer");
    var LocationIconID = "ControlPanelLocationButton" + myID;
    var LocationIcon = getDomOrCreateNew(LocationIconID);
    LocationIcon.setAttribute("Title","Location");
    $(LocationIcon).addClass("ControlPanelButton");
    $(LocationIcon).addClass("ControlPanelLocationButton");
    var ProcrastinateIconID = "ControlPanelProcrastinateButton" + myID;
    var ProcrastinateIcon = getDomOrCreateNew(ProcrastinateIconID);
    ProcrastinateIcon.setAttribute("Title", "Procrastinate");
    $(ProcrastinateIcon).addClass("ControlPanelButton");
    $(ProcrastinateIcon).addClass("ControlPanelProcrastinateButton");

    var DeleteIconID = "ControlPanelDeleteButton" + myID;
    var DeleteIcon = getDomOrCreateNew(DeleteIconID);
    DeleteIcon.setAttribute("Title", "Trash");
    $(DeleteIcon).addClass("ControlPanelButton");
    $(DeleteIcon).addClass("ControlPanelDeleteButton");

    var CompleteIconID = "ControlPanelCompleteButton" + myID;
    var CompleteIcon = getDomOrCreateNew(CompleteIconID);
    CompleteIcon.setAttribute("Title", "Mark as complete");
    $(CompleteIcon).addClass("ControlPanelButton");
    $(CompleteIcon).addClass("ControlPanelCompleteButton");

    var CloseIconID = "ControlPanelCloseButton" + myID;
    var CloseIcon = getDomOrCreateNew(CloseIconID);
    CloseIcon.setAttribute("Title", "Close Panel");
    $(CloseIcon).addClass("ControlPanelButton");
    $(CloseIcon).addClass("ControlPanelCloseButton");

    var PauseResumeIconID = "ControlPanelResumePauseButton" + myID;
    var PauseResumeIcon = getDomOrCreateNew(PauseResumeIconID);
    $(PauseResumeIcon).addClass("ControlPanelButton");

    var RepeatIconID = "ControlPanelRepeatButton" + myID;
    var RepeatIcon = getDomOrCreateNew(RepeatIconID);
    $(RepeatIcon).addClass("ControlPanelButton");
    $(RepeatIcon).addClass("ControlPanelRepeatButton");

    let NowIconId = "ControlPanelNowButton" + myID;
    let NowIcon =  getDomOrCreateNew(NowIconId);
    $(NowIcon).addClass("ControlPanelButton");
    $(NowIcon).addClass("ControlPanelNowButton");


    this.getCloseButton = function ()
    {
        return CloseIcon;
    }

    this.getPauseResumeButton = function () {
        return PauseResumeIcon;
    }

    this.getLocationButton = function () {
        return LocationIcon;
    }

    this.hideLocationButton = function () {
        $(LocationIcon).addClass("setAsDisplayNone");
    }
    this.showLocationButton = function () {
        $(LocationIcon).removeClass("setAsDisplayNone");
    }

    this.getDeleteButton = function () {
        return DeleteIcon;
    }

    this.hideDeleteButton = function () {
        $(DeleteIcon).addClass("setAsDisplayNone");
    }
    this.showDeleteButton = function () {
        $(DeleteIcon).removeClass("setAsDisplayNone");
    }

    this.getCompleteButton = function () {
        return CompleteIcon;
    }

    this.hideCompleteButton = function () {
        $(CompleteIcon).addClass("setAsDisplayNone");
    }
    this.showCompleteButton = function () {
        $(CompleteIcon).removeClass("setAsDisplayNone");
    }
    this.hideNowButton = function () {
        $(NowIcon).addClass("setAsDisplayNone");
    }
    this.showNowButton = function () {
        $(NowIcon).removeClass("setAsDisplayNone");
    }
    this.getProcrastinateButton = function () {
        return ProcrastinateIcon;
    }

    this.hideProcrastinateButton = function () {
        $(ProcrastinateIcon).addClass("setAsDisplayNone");
    }
    this.showProcrastinateButton = function () {
        $(ProcrastinateIcon).removeClass("setAsDisplayNone");
    }

    this.getIconSetContainer = function () {
        return IconSetContainer;
    }

    this.getRepeateButton = function () {
        return RepeatIcon;
    }

    this.getNowButton = function () {
        return NowIcon;
    }

    this.hideRepeatButton = function () {
        $(RepeatIcon).addClass("setAsDisplayNone");
    }
    this.showRepeatButton = function () {
        $(RepeatIcon).removeClass("setAsDisplayNone");
    }

    this.HidePausePauseResumeButton = function () 
    {
        $(PauseResumeIcon).addClass("setAsDisplayNone");
    }

    this.ShowPausePauseResumeButton = function () {
        $(PauseResumeIcon).removeClass("setAsDisplayNone");
    }

    this.switchToPauseButton = function () {
        $(PauseResumeIcon).addClass("ControlPanelPausePanelButton");
        $(PauseResumeIcon).removeClass("ControlPanelResumePanelButton");
        PauseResumeIcon.setAttribute("Title", "Pause Event");
    }

    this.switchToResumeButton = function () {
        $(PauseResumeIcon).addClass("ControlPanelResumePanelButton");
        $(PauseResumeIcon).removeClass("ControlPanelPausePanelButton");
        PauseResumeIcon.setAttribute("Title", "Resume Event");
    }

    IconSetContainer.appendChild(LocationIcon)
    IconSetContainer.appendChild(ProcrastinateIcon)
    IconSetContainer.appendChild(DeleteIcon)
    IconSetContainer.appendChild(CompleteIcon)
    IconSetContainer.appendChild(PauseResumeIcon)
    IconSetContainer.appendChild(RepeatIcon)
    IconSetContainer.appendChild(CloseIcon)
    IconSetContainer.appendChild(NowIcon)
    
}

IconSet.ID=0;


function multiSelect()
{
        var SelecedIDs = {};
        var EnableChange = true;
        var CallBackDict = {};
        var CallBackFunctions = [];
        var isMultiSelectActive = false;

        var AddElement = function (EventID)
        {
            var IniID = BindClickOfSideBarToCLick.ActiveID;
            if (!isMultiSelectActive) {
                global_ExitManager.triggerLastExitAndPop();
            }
            getRefreshedData.disableDataRefresh();
            if (SelecedIDs[EventID] == null)
            {
                if (IniID != null)
                {
                    AddElement(IniID);
                }
                
                
                var SubEvent = global_DictionaryOfSubEvents[EventID];
                SelecedIDs[EventID] = SubEvent;
                SelectElement(EventID);
                if (!isMultiSelectActive)
                {
                    global_ExitManager.addNewExit(resetAllElement);
                }
                isMultiSelectActive = true;
            }
            else
            {
                RemoveELement(EventID)
            }
            
            Change();
        }

        var RemoveELement = function (EventID)
        {
            if (!isMultiSelectActive)
            {
                global_ExitManager.triggerLastExitAndPop();
            }
            var SubEvent = SelecedIDs[EventID];
            
            if (SubEvent == null)
            {
                alert("oops Jay there is some discrepancy with your multiselect");
            }
            DeselectEleemnt(EventID);
            delete SelecedIDs[EventID];
            if (Object.keys(SelecedIDs).length < 1) {
                isMultiSelectActive = false;
            }
            Change();
        }

        var DisableChangeDetection = true;

        var resetAllElement = function ()
        {
            EnableChange = false;
            while (Object.keys(SelecedIDs).length > 0)
            {
                RemoveELement(Object.keys(SelecedIDs)[0])
            }

            /*
            if (Object.keys(SelecedIDs).length < 1) {
                isMultiSelectActive = false;
                global_ExitManager.triggerLastExitAndPop();
            }
            */

            isMultiSelectActive = false;
            EnableChange = true;
            Change();
            getRefreshedData.enableDataRefresh();
        }

        var SelectElement = function (EventID)
        {
            var SubEvent = SelecedIDs[EventID];
            SubEvent.gridDoms.forEach(HighlightSubEvent);
        }

        var DeselectEleemnt = function (EventID)
        {
            var SubEvent = SelecedIDs[EventID];
            SubEvent.gridDoms.forEach(dehighlightSubEvent);
        }

        var Change = function ()
        {
            if ((EnableChange))
            {
                if (isMultiSelectActive) {
                    CallBackFunctions.forEach(
                    function (myFunc) {
                        myFunc.CallBack(SelecedIDs);
                    });
                }
                else
                {
                    CallBackFunctions.forEach(
                    function (myFunc)
                    {
                        myFunc.Exit();
                    });
                }
                
            }
        }

        var CallAllExits= function()
        {
            
        }
        this.AddElement = AddElement;
        this.DeselectEleemnt = DeselectEleemnt;
        this.SelectElement = SelectElement;
        this.resetAllElement = resetAllElement;
        this.RemoveELement = RemoveELement;
        this.getAllSelectedElements = function () { SelecedIDs }
        this.getIsMultiActive = function ()
        {
            return isMultiSelectActive;
        }
        this.addCallBack = function (CallBack)
        {
            if (CallBackDict[CallBack] == undefined)
            {
                CallBackFunctions.push(CallBack);
                CallBackDict[CallBack] = CallBack;
            }
        }

        function removeCallBack (CallBack)
        {
            delete CallBackDict[CallBack];
            var funcIndex = CallBackFunctions.indexOf(CallBack);
            if (funcIndex > -1)
            {
                CallBackFunctions.splice(funcIndex, 1);
            }
        }
        
    

        this.removeCallBack = removeCallBack;
            
    
}

global_multiSelect = new multiSelect();
global_multiSelect.addCallBack(RevealControlPanelSection);

var FormatTime = function(date){
  var d = date;
  var TimeHours = d.getHours();
  var TimeMinutes = d.getMinutes(); 
  var TimeMonth = d.getMonth();
  var TimeMM = TimeMinutes;
  var TimeHH = TimeHours;
  var AMPM = 'am';
  var day = '';
  var month = '';
  var date_number = d.getDate();
  var year = d.getFullYear();

  if (TimeMinutes <= 9){
    TimeMM = '0' + TimeMinutes
  }
  if (TimeHours >= 12 && TimeMinutes >= 0){
    if (TimeHours >= 13){
      TimeHH = TimeHours - 12;
    }  
    AMPM = 'pm';
  } else if(TimeHours === 12 && TimeMinutes === 0) {
    TimeHH = 'Noon';
    AMPM = '';
    TimeMM = '';
  } else if(TimeHours === 24 && TimeMinutes === 0) {
    TimeHH = 'Midnight';
    AMPM= '';
    TimeMM = '';
  }
  switch (d.getMonth()){
    case 0:
      month = "Jan";
      break;
    case 1:
      month = "Feb";
      break;
    case 2:
      month = "Mar";
      break;
    case 3:
      month = "Apr";
      break;
    case 4:
      month = "May";
      break;
    case 5:
      month = "Jun";
      break;
    case 6:
      month = "Jul";
      break;
    case 7:
      month = "Aug";
      break;
    case 8:
      month = "Sep";
      break;
    case 9:
      month = "Oct";
      break;
    case 10:
      month = "Nov";
      break;
    case 11:
      month = "Dec";
      break;
  }
  switch (d.getDay()) {
    case 0:
      day = "Sunday";
      break;
    case 1:
      day = "Monday";
      break;
    case 2:
      day = "Tuesday";
      break;
    case 3:
      day = "Wednesday";
      break;
    case 4:
      day = "Thursday";
      break;
    case 5:
      day = "Friday";
      break;
    case 6:
      day = "Saturday";
      break;
  }
  var a = { hour:TimeHH, minute:TimeMM, merid:AMPM, day:day, mon:month, date:date_number, year:year, month_num:TimeMonth }
  return a;
            };

function generateDayContainer()
{
    var myID =++generateDayContainer.id;
    var DayContainer = getDomOrCreateNew("DayContainer" + myID);
    var DayContextContainer = getDomOrCreateNew("DayContextContainer" + myID);
    var SubEventListContainer = getDomOrCreateNew("SubEventListContainer" + myID);
    $(SubEventListContainer).addClass("SubEventListContainer");
    var MoreInfoPanel = getDomOrCreateNew("MoreInfoPanel" + myID);
    $(MoreInfoPanel).addClass("MoreInfoPanel");
    $(DayContextContainer).addClass("DayContextContainer");
    DayContainer.appendChild(DayContextContainer);
    DayContainer.onmouseover = onMouseIn;
    DayContainer.onmouseout = onMouseOut;
    
    $(DayContainer.Dom).addClass("DayContainer");
    var NameOfDayContainer = getDomOrCreateNew("NameOfDayContainer" + myID);
    NameOfDayContainer.onclick = function (e) { e.stopPropagation(); };
    var DayTimeContainer = getDomOrCreateNew("DayTimeContainer" + myID);//Single day grid displayed only on mouse over


    var FullGridContainer = getDomOrCreateNew("FullGridContainer" + myID);//Bar grid for classic view
    $(FullGridContainer.Dom).addClass("FullGridContainer");
    FullGridContainer.isEnabled = false;
    $(FullGridContainer).addClass("setAsDisplayNone");

    let SleepContainer = getDomOrCreateNew("SleepContainer" + myID);//Bar grid for classic view
    $(SleepContainer.Dom).addClass("SleepContainer");

    $(DayTimeContainer).addClass("setAsDisplayNone");
    $(NameOfDayContainer.Dom).addClass("NameOfDayContainer");
    DayContainer.Dom.appendChild(NameOfDayContainer.Dom);
    DayContextContainer.Dom.appendChild(FullGridContainer.Dom);//Full grid
    DayContextContainer.Dom.appendChild(DayTimeContainer.Dom);
    DayContextContainer.Dom.appendChild(SleepContainer.Dom);
    DayContextContainer.Dom.appendChild(MoreInfoPanel.Dom);
    DayContextContainer.Dom.appendChild(SubEventListContainer.Dom);

    $(DayTimeContainer.Dom).addClass("DayTimeContainer");
    var NumberOfShaders = 24;
    var TotalTopElement = 0;
    var ConstTopIncrement = 2;
    var aPm = [" am", " pm"];
    var TimeArray = [12,1,2,3,4,5,6,7,8,9,10,11]
    for (; NumberOfShaders > 0; --NumberOfShaders) {

        var shadeContainer = getDomOrCreateNew("shadeContainer" + myID + "" + NumberOfShaders);
        var FullBarShadeContainer = getDomOrCreateNew("FullGridShadeContainer" + myID + "" + NumberOfShaders);
        $(FullBarShadeContainer).addClass("FullGridShadeContainer");//shade for full grid
        var TimeOfDayTextContainer = getDomOrCreateNew("TimeOfDayTextContainer" + myID + "" + NumberOfShaders,"span");
        var Military = 24 - NumberOfShaders;
        var floorValue= Math.floor( Military / 12);
        var DayTime = TimeArray[Military % 12];
        var ampmIndex = DayTime != 12 ? "" : aPm[floorValue];
        
        var FullText = DayTime + ampmIndex;
        TimeOfDayTextContainer.innerHTML = FullText;
        $(TimeOfDayTextContainer).addClass("TimeOfDayText");
        shadeContainer.appendChild(TimeOfDayTextContainer);
        DayTimeContainer.Dom.appendChild(shadeContainer.Dom);
        FullGridContainer.appendChild(FullBarShadeContainer);
        
        shadeContainer.Dom.style.top = TotalTopElement + "%";
        FullBarShadeContainer.Dom.style.top = TotalTopElement + "%";
        TotalTopElement += 4.1667;
        $(shadeContainer.Dom).addClass("SingleDayShadeWidth");
        if (NumberOfShaders % 2) {
            $(shadeContainer.Dom).addClass("DarkerShade");
            $(FullBarShadeContainer.Dom).addClass("DarkerShade");
        }
        else {
            $(shadeContainer.Dom).addClass("LighterShade");
            $(FullBarShadeContainer.Dom).addClass("LighterShade");
        }

        $(shadeContainer.Dom).addClass("shadeContainer");
    }

    function revealMoreOptions()
    {
        $(MoreInfoPanel).removeClass("setAsDisplayNone")
        $(MoreInfoPanel).addClass("RevealMoreInfoPanel")
        AddCloseButoon(MoreInfoPanel, false);
    }

    function unRevealMoreOptions() {
        $(MoreInfoPanel).addClass("setAsDisplayNone");
        $(MoreInfoPanel).removeClass("RevealMoreInfoPanel")
        $(MoreInfoPanel).empty();
    }


    MoreInfoPanel.onclick=function(e)
    {
        e.stopPropagation();
    }
    unRevealMoreOptions();

    function onMouseIn()
    {
        setTimeout(function ()
        {
            if (FullGridContainer.isEnabled) {
                $(DayTimeContainer).addClass("setAsDisplayNone");
                return;
            }
            $(DayTimeContainer).removeClass("setAsDisplayNone");
        })
    }

    function onMouseOut()
    {
        setTimeout(function () {
            if (FullGridContainer.isEnabled) {
                $(DayTimeContainer).addClass("setAsDisplayNone");
                return;
            }
            $(DayTimeContainer).addClass("setAsDisplayNone");
        })
    }

    function generateSleepRenderFunction(sleepContainer) {
        return function(dayStart, timeLines) {
            let createSleepDom = () => {
                let domId = dayStart.getTime()+"_"+ generateUUID();
                let sleepDom = getDomOrCreateNew(domId).Dom
                sleepDom.classList.add('sleep-time-grid');

                return sleepDom;
            };
            if(timeLines) {
                let sleepDoms = new Set();
                let allTimes = [];
                timeLines.forEach((timeline) => {
                    allTimes.push(timeline.start.getTime());
                    allTimes.push(timeline.end.getTime());
                });
                allTimes.sort((a, b) => {return a - b});
                let hoverDaySleepStartString = new Date(allTimes[0]).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }); 
                let hoverDaySleepEndString = new Date(allTimes[allTimes.length - 1]).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) ;
                timeLines.forEach((timeLine, index) => {
                    let sleepStart = new Date(timeLine.start.getTime());
                    let sleepEnd = new Date(timeLine.end.getTime());
                    
                    sleepStart.setFullYear(dayStart.getFullYear());
                    sleepStart.setMonth(dayStart.getMonth(), dayStart.getDate());

                    sleepEnd.setFullYear(dayStart.getFullYear());
                    sleepEnd.setMonth(dayStart.getMonth(), dayStart.getDate());

                    let topPercent = ((sleepStart - dayStart)/OneDayInMs) * 100;
                    let heightPercent = ((sleepEnd - sleepStart)/OneDayInMs) * 100;
                    let sleepDom = sleepContainer.children[index];
                    if(!sleepDom) {
                        sleepDom = createSleepDom();
                        sleepContainer.appendChild(sleepDom);
                    }
                    sleepDom.style.height = heightPercent+"%";
                    sleepDom.style.top = topPercent+"%";

                    sleepDom.setAttribute("Title", "Snooze " + hoverDaySleepStartString+ " - " + hoverDaySleepEndString);

                    sleepDoms.add(sleepDom);
                })
            
                for(let i = 0; i < sleepContainer.children.length; i++) {
                    let nolongerValidSleepDom = sleepContainer.children[i];
                    if(!sleepDoms.has(nolongerValidSleepDom)) {
                        nolongerValidSleepDom.remove();
                    }
                }
                
            }
        }
    }

    var EventDayContainer = getDomOrCreateNew("EventDayContainer" + generateDayContainer.id);
    $(EventDayContainer.Dom).addClass("EventDayContainer");
    DayTimeContainer.Dom.appendChild(EventDayContainer.Dom);

    return {
        renderPlane: SubEventListContainer,
        FullDayContext: DayContextContainer,
        Parent: DayContainer,
        EventDayContainer: EventDayContainer,
        NameOfDayContainer: NameOfDayContainer,
        DayID: myID,
        MoreInfoPanel:MoreInfoPanel,
        RevealMoreOptions: revealMoreOptions,
        UnRevealMoreOptions: unRevealMoreOptions,
        RenderSleepSection: generateSleepRenderFunction(SleepContainer)
    };
}



generateDayContainer.id = 0;

//var RangeData;
var miscCounter = 0;

function BindAddButton()
{
    var AddButton = document.getElementById('AddEventButton');
    var newDate = new Date();
    newDate.setSeconds(0);
    newDate.setMinutes(0);
    $(AddButton).click(newAdd);
    function newAdd()
    {
        var HeaderContainer = getDomOrCreateNew("Header")
        var height = $(HeaderContainer).height();
        var width = $(HeaderContainer).width();
        generateModal(width/2, 0, 1, width, new Date(), HeaderContainer,true)
        //addNewEvent(0, 0, 0, newDate);
    }
}

function BindProcrastinateAllButton() {
    let procrastinateAll = getDomOrCreateNew('ProcrastinateAll');
    let newDate = new Date();
    newDate.setSeconds(0);
    newDate.setMinutes(0);
    $(procrastinateAll).click(procrastinate);
    function procrastinate()
    {
        ActivateUserSearch.setSearchAsOff();
        var HeaderContainer = getDomOrCreateNew("Header")
        var height = $(HeaderContainer).height();
        var width = $(HeaderContainer).width();
        generateProcrastinateAll(width/2, 0, 1, width, new Date(), HeaderContainer);
    }
}


function InitializeMonthlyOverview()
{
    BindAddButton();
    BindProcrastinateAllButton();
    SomethingNewButton(document.getElementById('SomethingNew'))
    initializeUserLocation();
    var verifiedUser = GetCookieValue();
    if (verifiedUser == "")
    {
        global_goToLoginPage();
        return;
    }

    UserCredentials.UserName = verifiedUser.UserName;
    UserCredentials.ID = verifiedUser.UserID;

    $('body').show();

    populateMonth();
    
    
    
    

    //genFunctionForSelectCalendarRange(GridRange, RefDate)();
    //getRefreshedData(GridRange);
    
    //global_ClearRefreshDataInterval=setTimeout(getRefreshedData, 0, GridRange);
}

function GenerateDayTime(LeftOrRight)
{
    var i = 0;
    var TimeOfDayContainer = getDomOrCreateNew("TimeOfDayContainer" + GenerateDayTime.counter);
    var percentagePerTop = 100 / 24;
   $(TimeOfDayContainer.Dom).addClass("TimeOfDayContainer");
    var AmText = getDomOrCreateNew("AmText", "span");
    AmText.Dom.innerHTML = "AM";
    var PmText = getDomOrCreateNew("PmText", "span");
    PmText.Dom.innerHTML = "PM";
    var amPmArray = ["<span>  AM  </span>", "<span>  PM  </span>"];
    for (; i < 24; i++)
    {
        var gridID = getDomOrCreateNew("DayTime" + GenerateDayTime.counter+"" + i);
        
        var stringee = "";
        if (LeftOrRight) {
            stringee=(i % 12 == 0 ? amPmArray[i / 12] + 12 : i % 12)+"";
            gridID.Dom.innerHTML = stringee;
        }
        else
        {
            stringee = (i % 12 == 0 ? 12 + amPmArray[i / 12] : i % 12) + "";
            gridID.Dom.innerHTML = i % 12 == 0 ? 12 +amPmArray[i / 12] : i % 12;
        }
        //gridID.Dom.appendChild(TimeOfDayContainer.Dom);
        gridID.Dom.style.top = (percentagePerTop * i) + "%";
        TimeOfDayContainer.Dom.appendChild(gridID.Dom);
        $(gridID.Dom).addClass("TimeOfDay");
    }

    return TimeOfDayContainer;
    
}

GenerateDayTime.counter = 0;

function populateMonth(refDate,CallBack)
{
    if (refDate == null)
    {
        refDate = Date.now();
    }
    refDate = new Date(refDate);
    global_WeekGrid = InitiateGrid(refDate);

    LaunchMonthTicker(refDate);
    function MyCallBack()
    {
        if(CallBack!=null)
        {
            CallBack();
        }
    }
    refreshCounter = 1
    getRefreshedData(MyCallBack);
    return global_WeekGrid;
}

function scrollToDay(refDate)
{
    var sampleDay = getDomOrCreateNew("DayContainer1");
    var refDateInMS = new Date(refDate).getTime();
    var WidthInPixels = $(".DayContainer").width();
    var CurrDay = global_CurrentRange.Start;
    var i = 0;
    var j = true;

    i = parseInt((refDateInMS - CurrDay) / OneDayInMs);

    if (j)
    {
        var bar = (i * parseInt(WidthInPixels)) - (WidthInPixels*3.5);
        WidthInPixels = bar;
        $("#NameOfWeekContainerPlane").animate({ scrollLeft: WidthInPixels }, 1000);
        $("#VerticalScrollContainer").animate({ scrollLeft: WidthInPixels }, 1000);
    }


}

function InitiateGrid(refDate)
{
    var encasingDOm = document.getElementById("FullWeekContainer");
    var LeftContainer = getDomOrCreateNew("leftDayOfTime");
    //$(LeftContainer.Dom).addClass("LefttimeOfDayContainer");
    
    var LeftDayOfTIme = GenerateDayTime(true); 
    LeftContainer.Dom.appendChild(LeftDayOfTIme.Dom);
    

    var RightContainer = getDomOrCreateNew("rightDayOfTime");
    var RightDayOfTIme = GenerateDayTime(false);
    RightContainer.Dom.appendChild(RightDayOfTIme.Dom);
    //(RightContainer.Dom).addClass("RightTimeOfDayContainer");



    var RangeData = PopulateUI(encasingDOm, refDate);
    var TimeBarContainer = document.getElementById("CurrentWeekContainer");
    $(LeftContainer.Dom).addClass("DayOfTime");
    $(RightContainer.Dom).addClass("DayOfTime");
    TimeBarContainer.appendChild(RightContainer.Dom);
    TimeBarContainer.appendChild(LeftContainer.Dom);
    
    return RangeData;
}


function onSocketDataReceipt(data) {
    console.log("yo son! " + onSocketDataReceipt.counter);
    if (!!data.refreshData) {
        if (data.refreshData.trigger) {
            refreshCounter = 1;
            console.log("refresh is  " + getRefreshedData.isEnabled);
            global_ExitManager.triggerLastExitAndPop();
            //getRefreshedData.enableDataRefresh();
            getRefreshedData();
        }
    }

    if (!!data.pauseData) {
        if (data.pauseData.pausedEvent) {
            refreshCounter = 1;
            console.log("refresh is  " + getRefreshedData.isEnabled);
            global_ExitManager.triggerLastExitAndPop();
            //getRefreshedData.enableDataRefresh();
            getRefreshedData();
        }
    }
    
    ++onSocketDataReceipt.counter
}
onSocketDataReceipt.counter = 0;

function initializeWebSockets() {
    var chat = $.connection.scheduleChange;
    //chat.client.sendToAll = onSocketDataReceipt;
    if(!!chat)
    {
        chat.client.refereshDataFromSockets = onSocketDataReceipt;
        $.connection.hub.start().done();
    }
    
}


function getRefreshedData(CallBackAfterRefresh)
{
    //setTimeout(refreshIframe,200);
    
    // if (--refreshCounter < 0)//debugging counter. THis allows us to set a max number of refreshes before stopping calls to backend
    // {
    //     return;
    // }
    refreshCounter = 0
    StopPullingData();
    monthViewResetData();
    var DataHolder = { Data: "" };
    if (getRefreshedData.isEnabled)
    {
        getRefreshedData.instanceCallBack = CallBackAfterRefresh;
        PopulateTotalSubEvents(DataHolder, global_WeekGrid, getRefreshedData.callAllCallbacks);
    }
    //global_ClearRefreshDataInterval = setTimeout(getRefreshedData, global_refreshDataInterval);

    return global_ClearRefreshDataInterval;
}
getRefreshedData.isEnabled = true;

getRefreshedData.disableDataRefresh = function ()
{
    getRefreshedData.isEnabled = false;
}

getRefreshedData.enableDataRefresh = function (pullLatest)
{
    getRefreshedData.isEnabled = true;
    if (pullLatest)
    {
        getRefreshedData();
    }

}

getRefreshedData.instanceCallBack = null;
getRefreshedData.callBacks = {};

getRefreshedData.callAllCallbacks = function (data) {
    for (var key in getRefreshedData.callBacks)
    {
        getRefreshedData.callBacks[key](TotalSubEventList);
    }
    
    getRefreshedData.callAllPauseCallbacks(data);
    if(isFunction(getRefreshedData.instanceCallBack)){
        getRefreshedData.instanceCallBack(data)
        getRefreshedData.instanceCallBack = null;
    }
}

getRefreshedData.callAllPauseCallbacks = function (data) {
    var pauseData = data.Content.Schedule.PauseData;
    for (var key in getRefreshedData.pauseCallBacks) {
        getRefreshedData.pauseCallBacks[key](pauseData);
    }
}

getRefreshedData.enroll = function (callback) {
    var Id = null;
    if (isFunction(callback)) {
        Id = generateUUID();
        if(!getRefreshedData.callBacks) {
            getRefreshedData.callBacks = {};
        }
        getRefreshedData.callBacks[Id] = callback;
    }
    else {
        throw "Non function provided when function is expected in getRefreshedData.Enroll"
    }
    return Id;
}

getRefreshedData.unEnroll = function (Id) {
    delete getRefreshedData.callBacks[Id]
}

getRefreshedData.pauseEnroll = function (callback) {
    var Id = null;
    if (isFunction(callback)) {
        Id = generateUUID();
        if (!getRefreshedData.pauseCallBacks) {
            getRefreshedData.pauseCallBacks = {};
        }
        getRefreshedData.pauseCallBacks[Id] = callback;
    }
    else {
        throw "Non function provided when function is expected in getRefreshedData.Enroll"
    }
    return Id;
}

getRefreshedData.pauseUnEnroll = function (Id) {
    delete getRefreshedData.pauseCallBacks[Id]
}



    function getEventsInterferringInRange(StartDate, EndDate)
    {
        var myTImeLine = { Start: StartDate, End: EndDate };
        var RetValue = getInterferringEventsWithTimeLine(myTImeLine, TotalSubEventList);
        return RetValue;
    }

    function getInterferringEventsWithTimeLine(TimeLine, CollectionOFEvents)
    {
        var retValue = new Array();
        CollectionOFEvents.forEach(
            function (obj) {
                if (isWithinTimeLine(TimeLine, obj)) {
                    retValue.push(obj)
                }
            });

        return retValue;

    }

    function isWithinTimeLine(TimeLine, Event)
    {
        var retvalue = false;

        var TimeLineStart = new Date ( TimeLine.Start);
        var TimeLineEnd = new Date ( TimeLine.End);
        var EventStart = new Date(Event.SubCalStartDate);
        var EventEnd = new Date(Event.SubCalEndDate);

        retvalue = ((EventStart <= TimeLineEnd) && (EventStart >= TimeLineStart)) || ((EventEnd <= TimeLineEnd) && (EventEnd >= TimeLineStart))
        return retvalue;
    }

    function refreshIframe(Start, End)
    {
        //var mapFrameDom = document.getElementsByName("MapContainerFrame");
        return;
        var AllEvents= getEventsInterferringInRange(Start, End);
        if (AllEvents.length > 0)
        {
            AllEvents.sort(function (a, b) { return (a.SubCalStartDate) - (b.SubCalStartDate) });

            var mapFrameDom = document.getElementById("MapFrame");
            var origin = AllEvents[0].SubCalAddress;
            var destination = AllEvents[AllEvents.length - 1].SubCalAddress;
            var WayPoints = generateWayPointString(AllEvents);
            var fullUrl = "https://www.google.com/maps/embed/v1/directions?key=AIzaSyAX5tvjWZixq3Qy_Rg9jATpjxUjf_Stftk&origin=" + origin + "&destination=" + destination + "&waypoints=" + WayPoints;
            mapFrameDom.src = fullUrl;
            mapFrameDom.src = mapFrameDom.src;
        }
    }

    function generateWayPointString(AllEvents)
    {
        var AllAddresses = [];

        AllEvents.forEach(function (obj) {
            AllAddresses.push(obj.SubCalAddress)
        })
        var retValue = AllAddresses.join("|");
        return retValue;
    }

    function deleteInActiveSubEvents()
    {

    }
    
    function PopulateTotalSubEvents(DataHolder, RangeData, CallBackAfterRefresh)
    {
        ///Gets the data from tiler back end. Also sucks out the subcalendar events

        var myurl = global_refTIlerUrl + "Schedule";
        var TimeZone = new Date().getTimezoneOffset();
        LoadingBar.showAllGroupings(weeklyScheduleLoadingBar);
        if (new Date().dst())
        {
            //TimeZone += 60;
        }
        getRefreshedData.disableDataRefresh();
        var PostData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, StartRange: RangeData.Start.getTime(), EndRange: RangeData.End.getTime(), TimeZoneOffset: TimeZone };
        preSendRequestWithLocation(PostData);
        $.ajax({
            type: "GET",
            url: myurl,
            data: PostData,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: prepFuncForPopulateSubEventData(DataHolder),
            error: function (err) {
                var myError = err;
                var step = "err";
            }
        }).done(function (response)
        {
            LoadingBar.hideAllGroupings(weeklyScheduleLoadingBar);
            //alert("done generating");
            if (CallBackAfterRefresh != null) {
                CallBackAfterRefresh(response);
            }
            PopulateMonthGrid(DataHolder.Data, RangeData);

            getRefreshedData.enableDataRefresh();
        });

        //    $.get(myurl, PopulateSubEventData);
    }


    function prepFuncForPopulateSubEventData(DataContainer)
    {
    //essentially returns a function that structuralizes the data for month grid
        return function (NewData)
        {
            var PerformanceStart = new Date();
            NewData = NewData.Content;
            ActiveSubEvents = new Array();
            var StructuredData = StructuralizeNewData(NewData)
            TotalSubEventList = StructuredData.TotalSubEventList;
            pageNotifications.processNotifications(TotalSubEventList);
            ActiveSubEvents = StructuredData.ActiveSubEvents;
            Dictionary_OfCalendarData = StructuredData.Dictionary_OfCalendarData;
            Dictionary_OfSubEvents = StructuredData.Dictionary_OfSubEvents;
            global_RemovedElemnts = global_DictionaryOfSubEvents;
            global_DictionaryOfSubEvents = {};
            DataContainer.Data = NewData;
            var PerformanceEnd = new Date();
            //console.log("Processing Data From Back End" + (PerformanceEnd - PerformanceStart));
        }

    }



    function GetDeletedEvents(RangeData, CallBackAfterRefresh) {
        ///Gets the data from tiler back end. Also sucks out the subcalendar events
        var myurl = global_refTIlerUrl + "Schedule/DeletedSchedule";
        var TimeZone = new Date().getTimezoneOffset();
        if (new Date().dst()) {
            //TimeZone += 60;
        }
        getRefreshedData.disableDataRefresh();
        var PostData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, StartRange: RangeData.Start.getTime(), EndRange: RangeData.End.getTime(), TimeZoneOffset: TimeZone };
        preSendRequestWithLocation(PostData);
        $.ajax({
            type: "GET",
            url: myurl,
            data: PostData,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: getSubEvents,
            error: function (err) {
                CallBackAfterRefresh();
            }
        }).done(function () {
            getRefreshedData.enableDataRefresh();
        });

        function getSubEvents(NewData)
        {
            NewData=NewData.Content
            var DeletedTotalSubEventList = StructuralizeNewData(NewData).TotalSubEventList;
            CallBackAfterRefresh(DeletedTotalSubEventList);
        }
    }


    


    function PopulateMonthGrid(NewData, RangeData)
    {
        var PerformanceStart = new Date();
        global_CurrentWeekArrangedData.forEach(
            function (WeekRange) {
                WeekRange.DaysOfWeek.forEach(
                function (DayOfWeek)
                {
                    for (var i in DayOfWeek.UISpecs)
                    {
                        DayOfWeek.UISpecs[i].Enabled = false;
                        DayOfWeek.UISpecs[i].Dom.Enabled = false;
                    }
            
                });
            });
        var PerformanceEnd = new Date();


        //console.log("Marking as false took " + (PerformanceEnd - PerformanceStart));

        PerformanceStart = new Date();
        TotalSubEventList.forEach(
            function (subEvent)
            {
                delete
                delete global_RemovedElemnts[subEvent.ID];
                getMyPositionFromRange(subEvent, RangeData);
            
                global_DictionaryOfSubEvents[subEvent.ID] = subEvent;
                global_DictionaryOfSubEvents[subEvent.ID].AllCallBacks = new Array();
            });
        PerformanceEnd = new Date();
        //console.log("Processing TotalSubEventList " + (PerformanceEnd - PerformanceStart));


        PerformanceStart = new Date();
        for (var ID in global_RemovedElemnts)
        {
            if (global_RemovedElemnts[ID].gridDoms != null) {


                global_RemovedElemnts[ID].gridDoms.forEach(function (eachDom) {
                    //console.log(eachDom.innerHTML);
                    if (eachDom.parentElement!=null)
                    {
                        eachDom.parentElement.removeChild(eachDom);
                    }
                    //outerHTML = "";
                    
                })
            }
            else
            {
                //alert("Jerome theres a problem");
            }
        }


        PerformanceEnd = new Date();
//        console.log("Removing elements from DOM " + (PerformanceEnd - PerformanceStart));

        PerformanceStart = new Date();
        RangeData.forEach(
            function (WeekRange) {
                for(var myKey in WeekRange.SubEventsCollection)
                {
                    var validSubEvents = WeekRange.SubEventsCollection[myKey];
                    getMyPositionFromRange(validSubEvents, WeekRange.DaysOfWeek);
                }

                WeekRange.DaysOfWeek.forEach(
                function (weekDay) {
                    for (var myKey in weekDay.SubEventsCollection)
                    {
                        var subEvent = weekDay.SubEventsCollection[myKey];
                        getMyDom(subEvent, weekDay);
                        
                    }
                });
            });
        PerformanceEnd = new Date();
        //console.log("Place SubEvents in expected weeks " + (PerformanceEnd - PerformanceStart));

        global_CurrentWeekArrangedData = RangeData;



        PerformanceStart = new Date();
        TriggerWeekUIupdate(global_CurrentWeekArrangedData);
        PerformanceEnd = new Date();
        return;
    }



    function TriggerWeekUIupdate(RangeData)
    {
        RangeData.forEach(
            function (WeekRange) {
                WeekRange.DaysOfWeek.forEach(prepareClearingOfOldUISubEVents);

        });
        RangeData.forEach(
            function (WeekRange) {
                WeekRange.DaysOfWeek.forEach(triggerSubEventRenderOnMonth);

            });
        RenderSleep();
    }


    function monthViewResetData()
    {
        
        global_WeekGrid.forEach(//clears the current assigned subevents for each week
           function (WeekRange) {
               WeekRange.SubEventsCollection = {};
               WeekRange.DaysOfWeek.forEach(
                function (weekDay) {
                    weekDay.SubEventsCollection = {};//clears the current assigned subevents for each day
                });
           });
    }

    function OnDragStartOfSubEvent(ev)
    {
        ev.dataTransfer.setData("text", ev.target.id);
    }

    function OnDraggingOfSubEvent(ev)
    {
        ev.preventDefault();
    }

    function OnDropOfSubEvent(ev)
    {
        ev.preventDefault();
        var data = ev.dataTransfer.getData("text");
        ev.target.appendChild(document.getElementById(data));
    }

    function renderNowUi (subEvent) {
        if (subEvent && subEvent.ListRefElement) {
            let currentSubEventClassName = "ListElementContainerCurrentSubevent";
            let ListElementContainer = subEvent.ListRefElement
            $(ListElementContainer.Dom).addClass(currentSubEventClassName);
            let nextSubEventTimeSpanInMs =  subEvent.SubCalEndDate.getTime() - Date.now();
            let nextSubEventIndex = TotalSubEventList.indexOf(subEvent)
            if(nextSubEventIndex >=0 && nextSubEventIndex < TotalSubEventList.length - 1) {
                ++nextSubEventIndex
                let nextSubEvent = TotalSubEventList[nextSubEventIndex]
                if(nextSubEventTimeSpanInMs >= OneMinInMs) {
                    setTimeout( () => {
                        $(ListElementContainer.Dom).removeClass(currentSubEventClassName);
                        renderNextUi(nextSubEvent);
                    },nextSubEventTimeSpanInMs)
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
            let ListElementContainer = nextSubEvent.ListRefElement;
            $(ListElementContainer.Dom).addClass(nextSubEventClassName);
            let timeSpanInMs = nextSubEvent.SubCalStartDate.getTime() - Date.now()
            setTimeout(() => {
                $(ListElementContainer.Dom).removeClass(nextSubEventClassName);
                getRefreshedData()
            }, timeSpanInMs)
        }
    }

function resetEventStatusUi() {
    //processes current subevent reset
    {
        let allCurrents = []
        let currentSubEventClassName = "ListElementContainerCurrentSubevent";
        let elements = $('.' + currentSubEventClassName)
        for (let i = 0; i < elements.length; i++) {
            let element = elements.get(i);
            $(element).removeClass(currentSubEventClassName);
        }
        global_UISetup.currentSubEvent= null
    }
    //processes next subevent reset
    {
        let nextSubEventClassName = "ListElementContainerNextSubevent";
        let elements = $('.' + nextSubEventClassName)
        for (let i = 0; i < elements.length; i++) {
            let element = elements.get(i);
            $(element).removeClass(nextSubEventClassName);
        }
        global_UISetup.nextSubEvent = null
    }
}

getRefreshedData.enroll(resetEventStatusUi);

    function triggerSubEventRenderOnMonth(DayOfWeek)
    {
        var verfyDate = new Date(2014, 5, 15, 0, 0, 0, 0);
        var a = 0;
        let foundNextEvent = false
        let now = Date.now()
        let isCurrentDayOfWeek = now < DayOfWeek.End.getTime() && now >= DayOfWeek.Start.getTime();
    

        var IntersectingArrayData = new Array();
        var SortedUISpecs = new Array();
        var Now = new Date();
        for (var ID in DayOfWeek.UISpecs)
        {
            if (DayOfWeek.UISpecs[ID].Enabled)
            {
                var SortedUISpecs_Element = DayOfWeek.UISpecs[ID];
                SortedUISpecs.push({SubEvent:SortedUISpecs_Element,ID: ID});
                
            }
        }

        
        SortedUISpecs.sort(function (a, b)
        {
            //var Diff = (a.SubEvent.Start) - (b.SubEvent.Start)
            return (a.SubEvent.Start) - (b.SubEvent.Start);
        });

        for (var i = 0; i < SortedUISpecs.length; i++)
        {
            var Element = SortedUISpecs[i];
            let subEvent = Element.SubEvent;
            let possibleNext = now < subEvent.Start;
            let isNext = false
            if(isCurrentDayOfWeek && possibleNext && !foundNextEvent) {
                foundNextEvent = true
                isNext = true
            }
            
            var myData = global_UISetup.RenderTimeInformation(DayOfWeek, Element.ID, isNext);
            
            IntersectingArrayData.push(myData);
        }


        //AllEvents.sort(function (a, b) { return (a.SubCalStartDate) - (b.SubCalStartDate) });
        //debugger;
        //IntersectingArrayData.sort(function (a, b) { return (a.Start) - (b.Start) });// no need for the line with SortedUISpecs.sort( does the job
        var MinPercent = ((1/24)* 100);//40 derived from min pixel height.
        
        var myIndex = 0;
        var MaxTabbingIndex = 1;
        var CallPrepSyncLater = [];
        for(var i=0;i<IntersectingArrayData.length;i++)
        {
            var ID = IntersectingArrayData[i].ID;
            if (DayOfWeek.UISpecs[ID].Enabled)
            {
                var widthSubtraction = 0;
                var LeftPercent = 0;
                DayOfWeek.UISpecs[ID].Dom.Active = true;
                //DayOfWeek.UISpecs[ID].Enabled = false;
                //DayOfWeek.UISpecs[ID].Dom.Enabled = false;
                
                var HeightPx=(DayOfWeek.UISpecs[ID].css.height/100)*global_DayHeight;
                var EndPixelTop =IntersectingArrayData[i].end;
                //IntersectingArrayData[i].BestBottom.End = EndPixelTop

                if (global_UISetup.ConflictCalculation(EndPixelTop, i, IntersectingArrayData))
                {
                    widthSubtraction += 10;
                }
                LeftPercent = IntersectingArrayData[i].Count*17;

                if (IntersectingArrayData[i].Count >= MaxTabbingIndex)
                {
                    MaxTabbingIndex = IntersectingArrayData[i].Count+1;
                }

                widthSubtraction += IntersectingArrayData[i].Count * 10;
                

                if (LeftPercent > 90)
                {
                    //LeftPercent = 90;
                }


                if (DayOfWeek.UISpecs[ID].OldIDindex!=null)
                {
                    var OldDomID = ID + "_" + DayOfWeek.UISpecs[ID].OldIDindex;// gets the old ID for the dom event
                    var toBeDeletedDom = getDomOrCreateNew(OldDomID);//gets the HTML element
                    if (!toBeDeletedDom.Dom.Active)//checks if it is active
                    {
                        toBeDeletedDom.Dom.style.height = 0;
                        $(toBeDeletedDom.Dom).hide();
                    }
                }


                DayOfWeek.UISpecs[ID].OldIDindex = DayOfWeek.UISpecs[ID].IDindex;
                //DayOfWeek.UISpecs[ID].css.width = (100 - widthSubtraction) * .1287;//12.87 derived from css file monthOverviewini.css. We chose a max width of 12.87%
                DayOfWeek.UISpecs[ID].css.left = (LeftPercent);
                
                DayOfWeek.UISpecs[ID].refSubEvent = IntersectingArrayData[i].refSubEvent;
                //$(DayOfWeek.UISpecs[ID].refrenceListElement.Dom).addClass("ListElement");
                

                //DayOfWeek.FullDayContext.Dom.appendChild(DayOfWeek.UISpecs[ID].Dom);
                DayOfWeek.WeekRenderPlane.appendChild(DayOfWeek.UISpecs[ID].Dom);
                
                
                $(DayOfWeek.UISpecs[ID].Dom).show();
                var CallBackLaterData = { ID: ID, Index: i };
                CallPrepSyncLater.push(CallBackLaterData);
                //setTimeout(renderSideBarEvents(DayOfWeek, ID, IntersectingArrayData, i, MaxTabbingIndex), 200 * ++a);
                DayOfWeek.maxIndex = global_ListOfDayCounter;

            }
            else
            {
                
            
            }
        }

        for (var i = 0 ; i < CallPrepSyncLater.length; i++)
        {
            var CallBackLaterData = CallPrepSyncLater[i]
            setTimeout(global_UISetup.RenderSubEvent(DayOfWeek, CallBackLaterData.ID, IntersectingArrayData, CallBackLaterData.Index, MaxTabbingIndex), 200 * ++a);
        }
        return;
    }


    function PositionIconSet(DayContainer, SubEventDom)
    {
        DayContainer = DayContainer.Parent;
        //var IconSetContainer = global_ControlPanelIconSet.getIconSetContainer();
        var ControlPanelContainer = getDomOrCreateNew("ControlPanelContainer");
        var documentWidth = $(document).width();
        var documentHeight = $(document).height();;
        var buffer = 20;
        var widthOfIconSet = $(ControlPanelContainer).width()
        var heightOfIconSet = $(ControlPanelContainer).height()
        var widthOfDayContainer = $(DayContainer).width();
        var widthOfListSubEvent = $(SubEventDom).width();
        var leftOfDayContainer = $(SubEventDom).offset().left;
        let RightOfDayContainer = leftOfDayContainer + widthOfListSubEvent;//540 is the width of the preview modal
        var topOfSubEvent = $(SubEventDom).offset().top;
        var PossibleBottom = topOfSubEvent + heightOfIconSet + 36 + 36;//extra 36 is save button and second 36 is for preview button
        var LeftOffset=0
        let isOnLeft = false
        let rightWidth = documentWidth - RightOfDayContainer;
        let leftWidth = leftOfDayContainer;
        if (leftWidth > rightWidth ) {
            LeftOffset = leftOfDayContainer - widthOfIconSet// - buffer
            isOnLeft = true
        }
        else
        {
            LeftOffset = leftOfDayContainer + widthOfListSubEvent + buffer;
        }

        if (PossibleBottom > documentHeight)
        {
            var Excess = PossibleBottom - documentHeight;
            var SubEventHeight = $(SubEventDom).height();
            topOfSubEvent -= (Excess + 45); //extra 40 is for save button. Save button is 36px
        }

        ControlPanelContainer.style.top = topOfSubEvent + "px";
        ControlPanelContainer.style.left = LeftOffset + "px";
        if(isOnLeft) {
            $(ControlPanelContainer).addClass('leftControlPanel');
            $(ControlPanelContainer).removeClass('rightControlPanel');
        } else {
            $(ControlPanelContainer).removeClass('leftControlPanel');
            $(ControlPanelContainer).addClass('rightControlPanel');
        }
        


        
        
        
        /*
        setTimeout(
            function ()
            {
                ControlPanelContainer.style.top = topOfSubEvent + "px";
                ControlPanelContainer.style.left = LeftOffset + "px";
            },
            300)*/
        AddCloseButoon(ControlPanelContainer, false);

        /*
        IconSetContainer.style.top = topOfSubEvent + "px";
        IconSetContainer.style.left = LeftOffset + "px";
        */

    }

    function BindClickOfSideBarToCLick(MyArray, FullContainer, Index, CLickedBar,EventID)
    {
        return function ()
        {
            if (!global_UISetup.RenderOnSubEventClick.isRefListSubEventClicked) {
                global_ExitManager.triggerLastExitAndPop();
            }
            else
            {
                BindClickOfSideBarToCLick.reset();
            }
            //global_multiSelect.resetAllElement();
            //global_multiSelect.removeCallBack(RevealControlPanelSection);
            /*
            if (BindClickOfSideBarToCLick.elementResetHeight != null)
            {
                BindClickOfSideBarToCLick.reset();
            }*/
            var ExpandElement = FullContainer.refrenceListElement.Dom
            var JustDomsFromMyArray = new Array();
            MyArray.forEach(function (element) { JustDomsFromMyArray.push( element.refSubEvent.Dom) })
            var increment = 20;
            var elementHeight = $(ExpandElement).height();
            BindClickOfSideBarToCLick.elementResetHeight = elementHeight;
            BindClickOfSideBarToCLick.clickedBar = CLickedBar;
            //ExpandElement.style.top = (Index * 20) + "px";
            BindClickOfSideBarToCLick.ExpandElement = ExpandElement;
            BindClickOfSideBarToCLick.DataElement = FullContainer.DataElement.Dom;
            //ExpandElement.style.height = (60) + "px";
            $(ExpandElement).addClass("FullColorAugmentation");

            //ExpandElement.style.backgroundColor = "yellow";
            HighlightSubEvent(FullContainer.DataElement.Dom);
            //$(FullContainer.DataElement.Dom).addClass("selectedElements");
            var resetArray = new Array();
            BindClickOfSideBarToCLick.previousIndex = Index;
            
            


            var AllPreviousSelection = $(".selectedDayElements");
            

            for(var i=0;i<AllPreviousSelection.length;i++)
            {
                 var obj=AllPreviousSelection[i]
                
                    var currIndex=JustDomsFromMyArray.indexOf(obj)
                    if (currIndex == -1)
                    {
                        $(obj).removeClass("selectedDayElements");
                        //$(obj).addClass("ListElements");
                        
                    }
            }
                

            for (var i = 0; i < MyArray.length; i++) {
                $(MyArray[i].refSubEvent.Dom).addClass("selectedDayElements");
            }

            for (var i = Index+1; i < MyArray.length; i++)
            {
                var CurrentTop = $(MyArray[i].refSubEvent.Dom).position().top;
                resetArray.push({ previousHeight: CurrentTop, DomElement: MyArray[i].refSubEvent.Dom,index:i })
                CurrentTop = (increment * i)+40;
                //MyArray[i].refSubEvent.Dom.style.top = CurrentTop + "px";
            }
            BindClickOfSideBarToCLick.resetArray = resetArray;
            BindClickOfSideBarToCLick.ActiveID = EventID;
            //global_ExitManager.addNewExit(BindClickOfSideBarToCLick.reset);
            //renderSubEventsClickEvents.ignoreTriggerLastExitAndPop = true;

            /*
            var myFunc = BindClickOfSideBarToCLick.reset;
            myFunc.isNotExitable = true;
            global_ExitManager.addNewExit(myFunc);
            setTimeout(function () {
                myFunc.isNotExitable = false;
            }, 100);
            */
        }
    }

    BindClickOfSideBarToCLick.reset = function ()
    {
        //BindClickOfSideBarToCLick.ExpandElement.style.height = BindClickOfSideBarToCLick.elementResetHeight + "px";
        //BindClickOfSideBarToCLick.ExpandElement.style.backgroundColor = "transparent";
        if (BindClickOfSideBarToCLick.ActiveID != null) {
            //BindClickOfSideBarToCLick.ExpandElement.style.top = (BindClickOfSideBarToCLick.previousIndex * 20) + "px";
            $(BindClickOfSideBarToCLick.clickedBar).removeClass("selectedElements")
            $(BindClickOfSideBarToCLick.DataElement).removeClass("selectedElements")
            $(BindClickOfSideBarToCLick.ExpandElement).removeClass("FullColorAugmentation");
            BindClickOfSideBarToCLick.resetArray.forEach(function (obj) {
                //obj.DomElement.style.top = (obj.index * 20) + "px";
            })
            BindClickOfSideBarToCLick.elementResetHeight = null;
            BindClickOfSideBarToCLick.ActiveID = null;
        }
        
    }
    
    var global_ListOfDayCounter = 0;

    function DoSideBarsInterSect(MeEnd, Index, AllElements)
    {
        var retValue = false;
        MeEnd = Number( MeEnd.toFixed(2));
        var Me = AllElements[Index];
        for (var i = Index+1; i < AllElements.length; i++)
        {
            var PossibleInterferringElement = AllElements[i];
            var PossibleInterferringTop =Number( PossibleInterferringElement.top.toFixed(2));
            if (PossibleInterferringTop < MeEnd)
            {
                //++Me.Count;
                PossibleInterferringElement.CalCCount = Me.CalCCount + 1;
                PossibleInterferringElement.Count = PossibleInterferringElement.CalCCount;
                PossibleInterferringElement.EarlierCount = PossibleInterferringElement.Count


                ///*// #### Note for revert to simplest tabbing enable on this comment block
                ///BestBottom is data on tab level which ends before Me. BestBottom.Count data member is the of the level, BestBOttom.End is the end pixel
                if (PossibleInterferringElement.BestBottom.End > MeEnd )
                {
                    PossibleInterferringElement.BestBottom.End = MeEnd
                    PossibleInterferringElement.BestBottom.Count = Me.Count;
                }
                if (PossibleInterferringTop >= Me.BestBottom.End)//checks to see if the conflicting Event can go to same level as the BestBottom of Me
                {
                    PossibleInterferringElement.Count=Me.BestBottom.Count
                    Me.BestBottom.End = PossibleInterferringElement.end;
                    PossibleInterferringElement.CalCCount = Me.CalCCount;
                }
                ///*/
                retValue = true;
            }
            else
            {
                break;
            }


            /*
            if (AllElements[Index].BestBottom.End <= myTop) {
                AllElements[i].Count = AllElements[Index].BestBottom.Count;
                AllElements[Index].BestBottom.End = End
            }*/
        }

        return retValue;
    }




    function DoSideBarsConflictClassic(MeEnd, Index, AllElements) {
        var retValue = false;
        MeEnd = MeEnd.toFixed(2);
        var Me = AllElements[Index];
        for (var i = Index + 1; i < AllElements.length; i++) {
            var PossibleInterferringElement = AllElements[i];
            var PossibleInterferringTop = PossibleInterferringElement.top.toFixed(2);
            if (PossibleInterferringTop < MeEnd) {
                ++Me.Count;
                ++Me.OverlappingCount;
                
                PossibleInterferringElement.PrecedingOverlapers = Me.PrecedingOverlapers;
                ++PossibleInterferringElement.PrecedingOverlapers;
                ++PossibleInterferringElement.OverlappingCount;
                ++Me.OverlappingAfterMe



                PossibleInterferringElement.CalCCount = Me.CalCCount + 1;
                PossibleInterferringElement.Count = PossibleInterferringElement.CalCCount;
                PossibleInterferringElement.EarlierCount = PossibleInterferringElement.Count


                ///*// #### Note for revert to simplest tabbing enable on this comment block
                ///BestBottom is data on tab level which ends before Me. BestBottom.Count data member is the of the level, BestBOttom.End is the end pixel
                if (PossibleInterferringElement.BestBottom.End > MeEnd) {
                    PossibleInterferringElement.BestBottom.End = MeEnd
                    PossibleInterferringElement.BestBottom.Count = Me.Count;
                }
                if (PossibleInterferringTop >= Me.BestBottom.End)//checks to see if the conflicting Event can go to same level as the BestBottom of Me
                {
                    PossibleInterferringElement.Count = Me.BestBottom.Count
                    Me.BestBottom.End = PossibleInterferringElement.end;
                    PossibleInterferringElement.CalCCount = Me.CalCCount;
                }
                ///*/
                retValue = true;
            }
            else {
                break;
            }
        }

        return retValue;
    }


    function DoSubEventsIntersect(MeEnd, Index, AllElements)
    {
        var retValue = false;
        MeEnd = MeEnd.toFixed(2);
        var Me = AllElements[Index];
        for (var i = Index+1; i < AllElements.length; i++)
        {
            var PossibleInterferringElement = AllElements[i];
            var PossibleInterferringTop = PossibleInterferringElement.top.toFixed(2);
            if (PossibleInterferringTop < MeEnd)
            {
                PossibleInterferringElement.CalCCount = Me.CalCCount + 1;
                PossibleInterferringElement.Count = PossibleInterferringElement.CalCCount

            }
        }
    }
    

    function HighlightSubEvent(Dom)
    {
        $(Dom).addClass("selectedElements");
    }

function dehighlightSubEvent(Dom)
{
    $(Dom).removeClass("selectedElements");
}

function renderClassicSubEventLook(DayOfWeek, ID, MyArray, Index, TabCount)
{
    return function () {
        //debugger;
        var GridSubEventWidth = renderClassicSubEventLook.PercentWidthOfDay;// / TabCount;
        var RefEvent = DayOfWeek.UISpecs[ID]

        var ArrayElement = MyArray[Index];
        var LeftPercent = DayOfWeek.LeftPercent;//DayOfWeek.RightPercent

        var NumberOfLeftShifts = ArrayElement.PrecedingOverlapers;
        var LeftShift = NumberOfLeftShifts;
        var DomWidth = (GridSubEventWidth - NumberOfLeftShifts)
        DomWidth -= ArrayElement.OverlappingAfterMe;

        LeftPercent = LeftPercent + LeftShift;
        $(RefEvent.Dom).addClass("ClassicGridEvents");
        RefEvent.Dom.setAttribute("tabindex", "0");
        RefEvent.Dom.style.left = (LeftPercent + 1) + "%"
        RefEvent.Dom.style.height = RefEvent.css.height + "%";

        RefEvent.Dom.style.width = (DomWidth) + "%";
        RefEvent.Dom.style.top = RefEvent.css.top + "%";
        
        var Range = global_DictionaryOfSubEvents[ID].Day;
        global_DictionaryOfSubEvents[ID].Bind = function () {  };

        function call_renderSubEventsClickEvents(e) {
            e.stopPropagation();
            if (e.ctrlKey) {
                global_multiSelect.AddElement(ID);
                return;
            }
            renderClassicSubEventsClickEvents(ID)
        }
    }

}
renderClassicSubEventLook.PercentWidthOfDay = (100 / 7) -1.5;

function renderSleepTimeSlot(DayOfWeek) {

}

function renderSideBarEvents(DayOfWeek, ID, MyArray, Index, TabCount)
{
    return function () {
        var RefEvent = DayOfWeek.UISpecs[ID];

        var GridSubEventWidth = renderSideBarEvents.SubEventGridWidthContainer / TabCount;
        $(RefEvent.Dom).addClass("SideBar");
        RefEvent.Dom.style.left = (DayOfWeek.RightPercent - GridSubEventWidth )+ "%"
        RefEvent.Dom.style.height = RefEvent.css.height + "%";
        //RefEvent.Dom.style.minHeight = (global_DayHeight * (1 / 24)) + "px";//1/24 because we want the minimum to be the size of an hour

        //RefEvent.Dom.style.marginLeft = (-(RefEvent.css.left + 18) + "px");
        RefEvent.Dom.style.marginLeft = (-(GridSubEventWidth * MyArray[Index].Count) + "%");
        RefEvent.Dom.style.width = GridSubEventWidth+"%";
        //RefEvent.Dom.style.width = RefEvent.css.width + "%";
        RefEvent.Dom.style.top = RefEvent.css.top + "%";
        if (RefEvent.IDindex ==0) {
            //triggers change to List elements UI elements
                //RefEvent.refrenceListElement.Dom.style.top = (Index * 20) + "px";
                //RefEvent.refrenceListElement.Dom.style.marginTop = (20) + "px";
                $(RefEvent.refrenceListElement.Dom).removeClass("FullColorAugmentation");
            //RefEvent.refrenceListElement.Dom.style.left = DayOfWeek.LeftPercent + "%";
                RefEvent.refrenceListElement.Dom.style.left = 0;
            if (global_DictionaryOfSubEvents[ID].ColorSelection > 0) {
                //$(RefEvent.refrenceListElement.Dom).addClass(global_AllColorClasses[global_DictionaryOfSubEvents[ID].ColorSelection].cssClass);
                $(RefEvent.DataElement.Dom).addClass(global_AllColorClasses[global_DictionaryOfSubEvents[ID].ColorSelection].cssClass);
                $(RefEvent.DataElement.Dom).addClass("subEventColor");
        }

    }
        var BindToThis = BindClickOfSideBarToCLick(MyArray, RefEvent, Index, RefEvent.Dom,ID);
        var Range = global_DictionaryOfSubEvents[ID].Day;
        RefEvent.refrenceListElement.Dom.onmouseover = function () {
            Range.Parent.onmouseover();
        }

        RefEvent.refrenceListElement.Dom.onmouseout = function () {
            Range.Parent.onmouseout();
        }


        //if statement ensures that only elements with IDIndex generate the function. This ensures that the selected element shows on the left side of the page.
        if ((global_DictionaryOfSubEvents[ID].Bind == null) && (RefEvent.IDindex == 0)) {
            global_DictionaryOfSubEvents[ID].Bind = BindToThis;
    }


        function call_renderSubEventsClickEvents(e)
        {
            e.stopPropagation();
            if (e.ctrlKey)
            {
                global_multiSelect.AddElement(ID);
                return;
            }
            global_UISetup.RenderOnSubEventClick(ID)
        }
        RefEvent.refrenceListElement.Dom.onclick = call_renderSubEventsClickEvents;
    }

}
renderSideBarEvents.PercentWidthOfDay = 100 / 7;
renderSideBarEvents.SubEventGridWidthContainer = (100 / 7) * (0.15);//0.15 be cause the SubEventListContainer uses a width of 85%

function renderSubEventsClickEvents(SubEventID)
{
    global_DictionaryOfSubEvents[SubEventID].Bind();
    global_DictionaryOfSubEvents[SubEventID].showBottomPanel();
    
    var DayContainer=global_DictionaryOfSubEvents[SubEventID].Day
    var refSubEvent = global_DictionaryOfSubEvents[SubEventID].ListRefElement
    setTimeout(function () { PositionIconSet(DayContainer, refSubEvent) },10);
}
renderSubEventsClickEvents.BottomPanelIsOpen = false;
renderSubEventsClickEvents.isRefListSubEventClicked = false;



function renderClassicSubEventsClickEvents(SubEventID) {
    //global_DictionaryOfSubEvents[SubEventID].Bind();
    global_DictionaryOfSubEvents[SubEventID].showBottomPanel();

    var DayContainer = global_DictionaryOfSubEvents[SubEventID].Day
    var refSubEvent = global_DictionaryOfSubEvents[SubEventID].TimeSizeDom
    setTimeout(function () { PositionIconSet(DayContainer, refSubEvent) }, 10);
}
renderClassicSubEventsClickEvents.BottomPanelIsOpen = false;
renderClassicSubEventsClickEvents.isRefListSubEventClicked = false;


function prepareClearingOfOldUISubEVents(DayOfWeek) {
    for (var ID in DayOfWeek.UISpecs) {
        DayOfWeek.UISpecs[ID].Dom.Active = false;
        var OldDomID = ID + "_" + DayOfWeek.UISpecs[ID].OldIDindex;
        var toBeDeletedDom = getDomOrCreateNew(OldDomID);//gets the HTML element
        toBeDeletedDom.Dom.Active = false
    }
}



function genFunctionForSelectCalendarRange(ArrayOfCalendars, RefDate) {
    return function () {
            ArrayOfCalendars.forEach(function (eachRange) {
                if ((RefDate.getTime() >= eachRange.Start) && (RefDate.getTime() < eachRange.End)) {
                    $(eachRange.Dom).show();
                }
                else {
                    $(eachRange.Dom).hide();
            }
        })
    }
}

function PopulateUI(ParentDom, refDate)// draws up the container and gathers all possible calendar within range.
{
    var StartWeekDateInMS = new Date(refDate);
    var StartOfWeekDay = StartWeekDateInMS.getDay();
    $(ParentDom).empty();

    StartOfWeekDay = 0 -StartOfWeekDay;
    var StartOfRange = new Date((StartWeekDateInMS.getTime() + (StartOfWeekDay * OneDayInMs)));
    StartOfRange.setHours(0, 0, 0, 0);
    var EndOfRange = new Date(StartOfRange.getTime());
    EndOfRange.setDate(EndOfRange.getDate() + (7  * global_RangeMultiplier));//sets the range to be used for query
    global_CurrentRange = {
        Start: StartOfRange,
        End: EndOfRange
    };

    var ScheduleRange = { 
        Start: StartOfRange, 
        End: EndOfRange
    };
    var CurrentWeek = ScheduleRange.Start;
    var AllRanges = new Array();
    var HorizontalScrollPlane = getDomOrCreateNew("HorizontalScrollPlane");
    //AllWeekData.style.width=(global_RangeMultiplier * 100) + "%"
    var NameOfWeekContainerPlane = getDomOrCreateNew("NameOfWeekContainerPlane");
    var AllWeekData = getDomOrCreateNew("CurrentWeekContainer");
    var VerticalScrollContainer = getDomOrCreateNew("VerticalScrollContainer");
    AllWeekData.appendChild(VerticalScrollContainer)
    var index = 0;
    while (CurrentWeek < ScheduleRange.End)
    {
        //debugger;
        //CurrentWeek = CurrentWeek.dst() ? new Date(Number(CurrentWeek.getTime()) +OneHourInMs) : CurrentWeek;
        let endTime = new Date(Number(CurrentWeek));
        endTime.setDate(endTime.getDate() + 7);
        var MyRange = { Start: CurrentWeek, End: endTime};


        var WeekGird = genDivForEachWeek(MyRange, AllRanges);
        var CurrentWeekDOm = WeekGird.WeekTwentyFourHourGrid;
        var CurrentNameWeekDOM = WeekGird.NameOfWeek;
        if (!CurrentWeekDOm.status) {
            NameOfWeekContainerPlane.appendChild(CurrentNameWeekDOM);
            VerticalScrollContainer.Dom.appendChild(CurrentWeekDOm.Dom);
            var translatePercent = 100 * index;
            CurrentWeekDOm.Dom.style.transform = 'translateX(' + translatePercent + '%)';
            $(CurrentWeekDOm.Dom).addClass("weekContainer");
            CurrentNameWeekDOM.Dom.style.transform = 'translateX(' + translatePercent + '%)';
            $(CurrentNameWeekDOM.Dom).addClass("weekContainer");
            CurrentWeekDOm.index = index;
            CurrentWeek = new Date(Number(CurrentWeek) + Number(OneWeekInMs));
        }
        ++index;
    }

    HorizontalScrollPlane.appendChild(NameOfWeekContainerPlane.Dom);
    HorizontalScrollPlane.appendChild(AllWeekData.Dom);
    ParentDom.appendChild(HorizontalScrollPlane)
    global_DayHeight = $(AllWeekData.Dom).height();
    global_WeekWidth = $(AllWeekData.Dom).width();
    global_DayTop = $(AllWeekData.Dom).offset().top;
    AllRanges.Start =StartOfRange;
    AllRanges.End =EndOfRange;
    return AllRanges;
}

function LaunchMonthTicker(CurrDate)
{
    if (CurrDate == null)
    {
        CurrDate = Date.now();
    }

        
    CurrDate = new Date(CurrDate.getFullYear(), CurrDate.getMonth(), 1);
    var MonthTickerData = generateAMonthBar(CurrDate);
    var MonthBarContainer = getDomOrCreateNew("MonthBar");
    MonthBarContainer.Dom.appendChild(MonthTickerData.Month.Dom);
}

function generateAMonthBar(MonthStart)
{
    //debugger;
    MonthStart = new Date(MonthStart);
    MonthSelection();
    var WholeMonthCOntainer = getDomOrCreateNew("MonthArrayContainer");
    var MonthSelectButton = getDomOrCreateNew("MonthButton");
    $(MonthSelectButton.Dom).addClass("MonthButton");
    MonthSelectButton.onclick = MonthSelection;
    MonthSelectButton.Dom.innerHTML = "<div>" + Months[MonthStart.getMonth()].substring(0, 3) + "</div>" + "<div>" + MonthStart.getFullYear() + "</div>";
    //WholeMonthCOntainer.Dom.appendChild(MonthSelectButton.Dom);
    MonthSelectButton.Date = MonthStart;
    MonthSelectButton.current = false;
    var AllDayContainer = getDomOrCreateNew("AllDayContainer");
    $(AllDayContainer).empty();
    $(AllDayContainer).addClass("MultipleBarSelectionContainer")
    //WholeMonthCOntainer.Dom.appendChild(AllDayContainer.Dom);
    var dayTicker = getDomOrCreateNew("DayTicker");
    $(dayTicker.Dom).addClass("Ticker");
    
    var AllDayDivs = genDaysForMonthBar(MonthStart);
    AllDayDivs.forEach(function (obj)
    {
        AllDayContainer.Dom.appendChild(obj.Dom)
        function CallBackFunc() {
            //debugger;
            scrollToDay(obj.StartDate);
            dayTicker.Dom.style.left = obj.left + "%";
        }
        //$(obj.Dom).click(CallBackFunc);
        obj.Dom.onclick = CallBackFunc;
        if ((new Date() >= obj.StartDate) && (new Date() < obj.EndDate))
        {
            setTimeout(function () { CallBackFunc() }, 200);
        }
        AllDayContainer.Dom.appendChild(dayTicker.Dom);
    });

    function goToDay(myDay)
    {
        var isDateWithin = (MonthStart.getTime() <= myDay.getTime()) && (myDay.getTime()<getNextMont(MonthStart).getTime());
        if (isDateWithin)
        {
            $(AllDayDivs[myDay.getDate() - 1]).trigger("click");
            return true;
        }
        return false;
    }
    global_GoToDay = goToDay;

    var retVAlue = { Days: AllDayDivs, Month: WholeMonthCOntainer, MonthButton: MonthSelectButton, DayContainer: AllDayContainer
        }
    return retVAlue;
}

function MonthSelection()
{
    MonthSelection.toggle();
    
    var AllMonthSelections = [];
    var MonthYearContainerID = "MonthYearContainer"
    var MonthYearContainer = getDomOrCreateNew(MonthYearContainerID);
    var AllMonthsContainerID = "AllMonthsContainer";
    var AllMonthsContainer = getDomOrCreateNew(AllMonthsContainerID);
    
    var AllYearContainerID = "AllYearContainer";
    var AllYearContainer = getDomOrCreateNew(AllYearContainerID);
    var leftPerRatio = 100 / 12;
    var MonthSelectedID = "MonthSelected";
    var MonthSelected = getDomOrCreateNew(MonthSelectedID);
    $(MonthSelected).addClass("MonthBar");
    var YearSelectedID = "YearSelected";
    var YearSelected = getDomOrCreateNew(YearSelectedID);
    $(YearSelected).addClass("YearBarDom");

    var RangeOfYearDelta = 3;

    MonthYearContainer.appendChild(AllMonthsContainer);
    MonthYearContainer.appendChild(AllYearContainer);
    generateOrCreateMonthBars();
    populateYearContainer(true);

    function generateOrCreateMonthBars()
    {
        if (AllMonthsContainer.status)
        {
            return;
        }
        var MonthID;
        var MonthButton;
        for (var i = 0; i < Months.length; i++)
        {
            MonthID = Months[i] + "bar";
            MonthButton = getDomOrCreateNew(MonthID);
            AllMonthsContainer.appendChild(MonthButton);
            $(MonthButton).addClass("MonthBar");
            MonthButton.style.left = (i * leftPerRatio) + "%";
            MonthButton.onclick = prepOnclickOfMonth(i, MonthButton);
            if (i == MonthSelection.CurrentSelection.Index)
            {
                updateMonthSelection(i, MonthButton);
                //MonthButton.onclick();
            }
            MonthButton.innerHTML = Months[i];
        }
        //AllMonthsContainer.appendChild(MonthSelected);

        
    }

    function populateYearContainer(UserCurrentDate)
    {
        
        var CurrentYear = MonthSelection.CurrentSelection.Year;
        var YearContainer = getDomOrCreateNew("YearDomContainer");
        $(YearContainer).empty();
        var AllYearDoms = [];
        var myYearID = "";
        var myYearDom = null;
        if (UserCurrentDate)
        {
            MonthSelection.Range.Start = CurrentYear - RangeOfYearDelta;
            MonthSelection.Range.End = CurrentYear + RangeOfYearDelta;
        }
        
        for (var i = MonthSelection.Range.Start; i <= MonthSelection.Range.End; i++)
        {
            myYearID = "YearBarDom" + i;
            myYearDom = getDomOrCreateNew(myYearID);
            myYearDom.innerHTML = i;
            $(myYearDom).addClass("YearBarDom");
            myYearDom.onclick = prepOnclickOfYear(myYearDom,i)
            AllYearDoms.push(myYearDom);
            YearContainer.appendChild(myYearDom);
            
        }
        myYearID = "YearBarDom" + CurrentYear
        $(AllYearContainer).prepend(YearContainer);
        //YearContainer.appendChild(YearSelected);
        var currentYearDom = getDomOrCreateNew(myYearID);
        if (currentYearDom.status)
        {
            currentYearDom.onclick()
        }
        generateOrCreateYearBars();
    }


    function generateOrCreateYearBars()
    {
        if (AllYearContainer.status) {
            return;
        }
        var YearID;
        var LeftYearButton = getDomOrCreateNew("LeftYearScrollButoon");
        var RightYearButton = getDomOrCreateNew("RightYearScrollButoon");
        $(LeftYearButton).addClass("yearScrollButton");
        $(RightYearButton).addClass("yearScrollButton");
        var LeftInnerArrow = getDomOrCreateNew("LeftInnerArrow");
        var RightInnerArrow = getDomOrCreateNew("RightInnerArrow");
        $(RightInnerArrow).addClass("innerArrow");
        $(LeftInnerArrow).addClass("innerArrow");

        LeftYearButton.appendChild(LeftInnerArrow);
        RightYearButton.appendChild(RightInnerArrow);

        AllYearContainer.appendChild(LeftYearButton);
        LeftYearButton.onclick = LeftScrollButtonClick;
        AllYearContainer.appendChild(RightYearButton);
        RightYearButton.onclick = RightScrollButtonClick;

        function RightScrollButtonClick()
        {
            ++MonthSelection.Range.Start;
            ++MonthSelection.Range.End;
            populateYearContainer()
        }
        function LeftScrollButtonClick()
        {
            --MonthSelection.Range.Start;
            --MonthSelection.Range.End;
            populateYearContainer();
        }
    }

    

    function prepOnclickOfMonth(Index,MonthDom)
    {
        function onClickMonth()
        {
            updateMonthSelection(Index, MonthDom);
            MonthSelection.HideAndCheckForChanges(true);

        }
        return onClickMonth;
    }



    function updateMonthSelection(Index, MonthDom)
    {
        //MonthSelected.style.left = (Index * leftPerRatio) + "%";
        if (MonthSelection.CurrentSelection.MonthDom!=null)
        {
            $(MonthSelection.CurrentSelection.MonthDom).removeClass("MonthSelected");
        }
        $(MonthDom).addClass("MonthSelected")
        MonthSelection.CurrentSelection.Month = Months[Index].substring(0, 3);
        MonthSelection.CurrentSelection.Index = Index;
        MonthSelection.CurrentSelection.MonthDom = MonthDom;
    }

    function prepOnclickOfYear(YearDom,Year)
    {
        function onClickYear()
        {
            //debugger;
            if (MonthSelection.CurrentSelection.YearDom != null)
            {
                $(MonthSelection.CurrentSelection.YearDom).removeClass("YearSelected");
            }
            $(YearDom).addClass("YearSelected");
            //var leftOfYearDom = $(YearDom).position().left;
            //YearSelected.style.left = leftOfYearDom+"px";
            MonthSelection.CurrentSelection.Year = Year;
            MonthSelection.CurrentSelection.YearDom = YearDom;
        }

        return onClickYear;
    }

    function ShowMonthList()
    {
        
    }
    var MonthID=0

    function genCallBackForEachMonth(Month)
    {
        var myMonthID = MonthID++;
        var monthStringID = Month + myMonthID;
        var MonthSelectionDiv = getDomOrCreateNew(monthStringID);
        MonthSelectionDiv.innerHTML = Month;
    }

    function PrepFunctionForClick(MonthDom, MonthIndex)
    {
        return function ()
        {
            if (MonthSelection.Index>-1)
            {
                $(MonthSelection.CurrentMonth).removeClass("SelectedMonth");
            }
            MonthSelection.CurrentMonth = MonthDom;
            MonthSelection.Index = MonthIndex;
        }
    }
}

MonthSelection.CurrentSelection = { Month: Months[new Date().getMonth()], Index: new Date().getMonth(), MonthDom: null,YearDom:null, Year: new Date().getFullYear(), SelectedYearID: -1, YearID: 0 };
MonthSelection.Range = { Start: new Date().getFullYear() - 3, End: new Date().getFullYear() + 3 };
MonthSelection.isOpen = true;
MonthSelection.initData = { Month: MonthSelection.CurrentSelection.Index, Year: MonthSelection.CurrentSelection.Year }
MonthSelection.Reveal = function ()
{
    global_ExitManager.triggerLastExitAndPop();
    var MonthYearContainer = getDomOrCreateNew("MonthYearContainer");
    $(MonthYearContainer).slideDown(0);
    MonthSelection.isOpen = true;
    MonthSelection.initData = { Month: MonthSelection.CurrentSelection.Index, Year: MonthSelection.CurrentSelection.Year }
    global_ExitManager.addNewExit(MonthSelection.Hide);
}

MonthSelection.Hide = function (CheckChange)
{
    var MonthYearContainer = getDomOrCreateNew("MonthYearContainer");
    $(MonthYearContainer).slideUp(0);
    MonthSelection.isOpen = false;
}

MonthSelection.HideAndCheckForChanges = function (CheckChange)
{
    global_ExitManager.triggerLastExitAndPop();
    if (MonthSelection.isChanged() && CheckChange)
    {
        //debugger;
        var myDate = new Date(MonthSelection.CurrentSelection.Year, MonthSelection.CurrentSelection.Index, 1);
        populateMonth(myDate);
    }
}

MonthSelection.toggle = function ()
{
    if (MonthSelection.isOpen) {
        MonthSelection.Hide();
    }
    else
    {
        MonthSelection.Reveal();
    }
}


MonthSelection.isChanged = function ()
{
    if ((MonthSelection.initData.Month != MonthSelection.CurrentSelection.Index) || (MonthSelection.initData.Year != MonthSelection.CurrentSelection.Year))
    {
        //alert("Hey Jay Making a month Change");
        return true;
    }
    return false;
}



function genDaysForMonthBar(MonthStart)
{
    //function creates the day divs in the month bar 
    var Month = MonthStart.getMonth() +1;
    var IniMonth =Month;
    var Day = MonthStart.getDate();

    var TodayStart = new Date(Date.now());
    TodayStart.setHours(0);
    TodayStart.setMinutes(0);
    TodayStart.setSeconds(0);
    TodayStart.setMilliseconds(0);
    var TodayInMS = TodayStart.getTime();
    var AllDayDiivs = new Array();
    var i = 0;
    while (IniMonth == Month)
    {
        var isToday = MonthStart.getTime() == TodayInMS;
        var MyDivContainer = getDomOrCreateNew("MonthBarDayWeekContainer" + genDaysForMonthBar.Day++);
        var MyDay = getDomOrCreateNew("MonthBarDay" + genDaysForMonthBar.Day++);
        var MyDivWeekDay = getDomOrCreateNew("MonthBarDayOfWeek" + genDaysForMonthBar.Day++);
        MyDay.Dom.innerHTML = Day;
        MyDivWeekDay.Dom.innerHTML =WeekDays[MonthStart.getDay()][0];

        MyDivContainer.Dom.appendChild(MyDay.Dom);
        MyDivContainer.Dom.appendChild(MyDivWeekDay.Dom);

        $(MyDay.Dom).addClass("MonthBarDay");
        $(MyDivWeekDay.Dom).addClass("MonthBarWeekDay")
        $(MyDivContainer.Dom).addClass("MonthBarDayWeekContainer");
        var LeftPosition = (i++ * 3.22);
        MyDivContainer.Dom.style.left = LeftPosition + "%";
        MyDivContainer.left = LeftPosition


        MyDivContainer.StartDate = MonthStart;
        MonthStart = new Date(MonthStart.getFullYear(), MonthStart.getMonth(), ++Day);
        MyDivContainer.EndDate = new Date(MonthStart.getTime() -1);
        AllDayDiivs.push(MyDivContainer);
        if (isToday) {
            var PreviousDay = $(".CurrentDayMonthBar");
            PreviousDay.removeClass("CurrentDayMonthBar");
            $(MyDivContainer).addClass("CurrentDayMonthBar");
            $(MyDivWeekDay.Dom).addClass("CurrentDayMonthBar")
        }

        Month = MonthStart.getMonth() +1;
    }

    return AllDayDiivs;
}

genDaysForMonthBar.Day = 0;

generateAMonthBar.counter = 0;

function getMyPositionFromRange(SubEvent, AllRangeData)//figures out what range to assign a subEvent. THe range could be a week, month ,or day. It returns an array for the corresponding range it might fall within
{
    //var AllRangeData = new Array();
    var i = 0;

    var verfyDate = new Date(2014, 5, 18, 0, 0, 0, 0);


    SubEvent.CurrentIndex = 0;
    var SubEventStart = new Date(Number(SubEvent.SubCalStartDate));
    var SubEventEnd = new Date(Number(SubEvent.SubCalEndDate));
    var ValidRange = new Array();

    for (i in AllRangeData)
    {
        var rangeElement = AllRangeData[i]
        var StartDate = new Date(Number(rangeElement.Start));
        var EndDate = new Date(Number(rangeElement.End));

        if (((SubEvent.ID === "11792_7_11978_11979")))
        {
                var a = 9;
        }


        if ((SubEventStart >= StartDate) && (SubEventStart <= EndDate))
        {
            ValidRange.push(rangeElement);
            rangeElement.SubEventsCollection[SubEvent.ID] = (SubEvent);

            if (rangeElement.UISpecs[SubEvent.ID] == null)
            {
                rangeElement.UISpecs[SubEvent.ID] = { css: { }, IDindex: SubEvent.CurrentIndex, OldIDindex: null, Enabled: true, Dom: null}
            }
            rangeElement.UISpecs[SubEvent.ID].Start = SubEvent.SubCalStartDate.getTime();
            rangeElement.UISpecs[SubEvent.ID].IDindex = SubEvent.CurrentIndex;
            rangeElement.UISpecs[SubEvent.ID].Enabled = true;
            rangeElement.UISpecs[SubEvent.ID].Dom = null;

        }
        if ((SubEventEnd >= StartDate) && (SubEventEnd <= EndDate)) {
            if (ValidRange.indexOf(rangeElement) < 0)
            {
                ValidRange.push(rangeElement);
                if (rangeElement.UISpecs[SubEvent.ID] == null)
                {
                    rangeElement.UISpecs[SubEvent.ID] = { css: { }, IDindex: SubEvent.CurrentIndex, OldIDindex: null, Enabled: true, Dom: null}
                }

                rangeElement.SubEventsCollection[SubEvent.ID] = SubEvent;
                rangeElement.UISpecs[SubEvent.ID].IDindex = SubEvent.CurrentIndex;
                rangeElement.UISpecs[SubEvent.ID].Enabled = true;
                rangeElement.UISpecs[SubEvent.ID].Dom = null;
                rangeElement.UISpecs[SubEvent.ID].Start = SubEvent.SubCalStartDate.getTime();
            }
        }
    }


    if (ValidRange.length > 0) 
    {
        return ValidRange;
    }

    return ValidRange;
}




        function getMyDom(SubEvent, Range) 
        {
            var referenceStart = new Date(Range.Start);
                //referenceStart.setHours(refStart.Hour, refStart.Minute);
            var referenceEnd = new Date(Range.End);
                //referenceEnd.setHours(refEnd.Hour, refEnd.Minute);
            var SubCalCalEventStart = new Date(SubEvent.SubCalStartDate);
            var SubCalCalEventEnd = new Date(SubEvent.SubCalEndDate);
            if ((SubEvent.gridDoms == undefined) || (SubEvent.gridDoms == null))
            {
                SubEvent.gridDoms = new Array();
            }

            if (SubCalCalEventStart > referenceStart) {
                referenceStart = SubCalCalEventStart;
            }

            if (referenceEnd > SubCalCalEventEnd) {
                referenceEnd = SubCalCalEventEnd;
            }

            var totalDuration = referenceEnd -referenceStart;
            var percentHeight = (totalDuration / OneDayInMs) * 100;
            let startOfDay = new Date(referenceStart.getFullYear(), referenceStart.getMonth(), referenceStart.getDate())
            var percentTop = ((referenceStart - startOfDay) / OneDayInMs) * 100;
            function call_renderSubEventsClickEvents(e)
            {
                e.stopPropagation();
                //renderSubEventsClickEvents(SubEvent.ID)
                global_UISetup.RenderOnSubEventClick(SubEvent.ID);
            }
            if (Range.UISpecs[SubEvent.ID].Enabled)
            {
                Range.UISpecs[SubEvent.ID].css = { height: 0, top: 0};
                Range.UISpecs[SubEvent.ID].css.height = percentHeight;
                Range.UISpecs[SubEvent.ID].css.top = percentTop;
                Range.UISpecs[SubEvent.ID].Enabled = true;
                var CurrentIndex = SubEvent.CurrentIndex++;
                Range.UISpecs[SubEvent.ID].IDindex = CurrentIndex;


                var EventDom = getDomOrCreateNew(SubEvent.ID + "_" + Range.UISpecs[SubEvent.ID].IDindex);

                Range.UISpecs[SubEvent.ID].Dom = EventDom.Dom
                Range.UISpecs[SubEvent.ID].Dom.Enabled = true;
                SubEvent.gridDoms.push(EventDom.Dom);

                $(EventDom.Dom).addClass("gridSubevent");

                EventDom.setAttribute("Title", SubEvent.SubCalStartDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + SubEvent.SubCalEndDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
                //debugger;
                if (SubEvent.ColorSelection > 0)
                {
                    $(EventDom.Dom).addClass(global_AllColorClasses[SubEvent.ColorSelection].cssClass);
                }
                $(EventDom.Dom).addClass("SameSubEvent" + SubEvent.ID);

                //$(EventDom.Dom).click(prepOnClickOfCalendarElement(SubEvent, EventDom.Dom));
                global_DictionaryOfSubEvents[SubEvent.ID].showBottomPanel = prepOnClickOfCalendarElement(SubEvent, EventDom.Dom);
                global_DictionaryOfSubEvents[SubEvent.ID].Day = Range;
                global_DictionaryOfSubEvents[SubEvent.ID].TimeSizeDom = EventDom;

                EventDom.Dom.onmouseover = function () {
                    //debugger;
                    Range.Parent.onmouseover();
                }

                EventDom.Dom.onmouseout = function () {
                    //debugger;
                    Range.Parent.onmouseout();
                }

                //global_DictionaryOfSubEvents[SubEvent.ID].


                EventDom.Dom.onclick = call_renderSubEventsClickEvents;
                //EventDom.Dom.innerHTML = SubEvent.Name
            }
            else
            {
                if (!Range.UISpecs[SubEvent.ID].Enabled)//checks if it is active
                {
                    debugger;
                    Range.UISpecs[SubEvent.ID].Dom.outerHTML ="";
                    //Range.UISpecs[SubEvent.ID].Dom.style.height = 0;
                    var retValue = delete Range.UISpecs[SubEvent.ID];
                    //toBeDeletedDom.Dom.parentElement.removeChild(toBeDeletedDom.Dom);
                }
            }
        }


        function hideControlInfoContainer()
        {
            var InfoContainer = getDomOrCreateNew("InfoContainer");
            $(InfoContainer).addClass("setAsDisplayNone");
        }

        function revealControlInfoContainer() {
            var InfoContainer = getDomOrCreateNew("InfoContainer");
            $(InfoContainer).removeClass("setAsDisplayNone");
        }

        function removeLowerBarIconSetCOntainer()
        {
            $(global_ControlPanelIconSet.getIconSetContainer()).removeClass("ControlPanelIconSetContainerLowerBar");
            $(getDomOrCreateNew("InfoBasePanel")).removeClass("setAsDisplayNone");
        }

        function addLowerBarIconSetCOntainer()
        {
            $(global_ControlPanelIconSet.getIconSetContainer()).addClass("ControlPanelIconSetContainerLowerBar")
            $(getDomOrCreateNew("InfoBasePanel")).addClass("setAsDisplayNone");
        }

        var global_previousSelectedSubCalEvent = new Array();

        function DeselectAllSideBarElements()
        {
            for (var i = 0; i < global_previousSelectedSubCalEvent.length; i++) {
                var myDom = global_previousSelectedSubCalEvent[i];
                $(myDom).removeClass("SelectedWeekGridSubcalEvent");
            }
            global_previousSelectedSubCalEvent = new Array();
        }


        function isProcrastinateInputValueVallid () {
            var HourInput = getDomOrCreateNew("procrastinateHours").value == "" ? 0 : getDomOrCreateNew("procrastinateHours").value;
            var MinInput = getDomOrCreateNew("procrastinateMins").value == "" ? 0 : getDomOrCreateNew("procrastinateMins").value;
            var DayInput = getDomOrCreateNew("procrastinateDays").value == "" ? 0 : getDomOrCreateNew("procrastinateDays").value;
            let durationInMs = (OneHourInMs * HourInput) + (MinInput * OneMinInMs) + (DayInput * OneDayInMs);
            let retValue = durationInMs > 0;
            return retValue;
        }

        function getProcrastinateSingleEventData(SubEvent) {
            var HourInput = getDomOrCreateNew("procrastinateHours").value == "" ? 0 : getDomOrCreateNew("procrastinateHours").value;
            var MinInput = getDomOrCreateNew("procrastinateMins").value == "" ? 0 : getDomOrCreateNew("procrastinateMins").value;
            var DayInput = getDomOrCreateNew("procrastinateDays").value == "" ? 0 : getDomOrCreateNew("procrastinateDays").value;
            let durationInMs = (OneHourInMs * HourInput) + (MinInput * OneMinInMs) + (DayInput * OneDayInMs);
            let isValid = durationInMs > 0
            var TimeZone = new Date().getTimezoneOffset();
            var retValue = { UserName: UserCredentials.UserName, 
                UserID: UserCredentials.ID,
                EventID: SubEvent.ID, 
                DurationInMs: durationInMs, 
                TimeZoneOffset: TimeZone,
                isValid: isValid
            };
            retValue.TimeZone = moment.tz.guess();
            return retValue;
        }

        function getProcrastinateAllData() {
            var HourInput = getDomOrCreateNew("procrastinateHours").value == "" ? 0 : getDomOrCreateNew("procrastinateHours").value;
            var MinInput = getDomOrCreateNew("procrastinateMins").value == "" ? 0 : getDomOrCreateNew("procrastinateMins").value;
            var DayInput = getDomOrCreateNew("procrastinateDays").value == "" ? 0 : getDomOrCreateNew("procrastinateDays").value;
            let durationInMs = (OneHourInMs * HourInput) + (MinInput * OneMinInMs) + (DayInput * OneDayInMs);
            let isValid = durationInMs > 0
            var TimeZone = new Date().getTimezoneOffset();
            var retValue = { 
                UserName: UserCredentials.UserName, 
                UserID: UserCredentials.ID,
                DurationInMs: durationInMs, 
                TimeZoneOffset: TimeZone,
                isValid: isValid
            };
            retValue.TimeZone = moment.tz.guess();
            return retValue;
        }

        function getSubeventUpdateData(SubEvent) {
            var TimeZone = new Date().getTimezoneOffset();
            let SubEventStartTime = getDomOrCreateNew("StartTimeInput", "input");
            let SubEventEndTime = getDomOrCreateNew("EndTimeInput", "input");
            let SubEventStartDate = getDomOrCreateNew("SubEventStartDateInput", "input");
            let SubEventEndDate = getDomOrCreateNew("SubEventEndDateInput", "input");

            let CalEndTime = getDomOrCreateNew("CalEndTime", "input");
            let CalEndDate = getDomOrCreateNew("CalEndDate", "input");

            let NameContanierInput = getDomOrCreateNew("NameInputBox", "input");

            SubEventStartTime.value =formatTimePortionOfStringToRightFormat(SubEventStartTime.value )
            let SubCalStartDateTimeString = SubEventStartTime.value.trim() + " " + $(SubEventStartDate).datepicker("getDate").toLocaleDateString().trim();
            let SubCalStartDateInMS = moment(SubCalStartDateTimeString, "hh:mm a MM/DD/YYYY").toDate().getTime();
            


            SubEventEndTime.value = formatTimePortionOfStringToRightFormat(SubEventEndTime.value)
            let SubCalEndDateTimeString = SubEventEndTime.value.trim() + " " + $(SubEventEndDate).datepicker("getDate").toLocaleDateString().trim();
            let SubCaEndDateInMS = moment(SubCalEndDateTimeString, "hh:mm a MM/DD/YYYY").toDate().getTime();
            
            CalEndTime.value = formatTimePortionOfStringToRightFormat(CalEndTime.value)

            let CalDateEndTimeString = CalEndTime.value.trim() + " " + $(CalEndDate).datepicker("getDate").toLocaleDateString().trim();
            let CalEndDateInMS = moment(CalDateEndTimeString, "hh:mm a MM/DD/YYYY").toDate().getTime();
            
            let splitValue = Number(getDomOrCreateNew("InputSplitCount", "input").value);
            

            let notesDom = getDomOrCreateNew("notesArea");
            let Notes = notesDom.value || SubEvent.Notes
            let retValue = {
                UserName: UserCredentials.UserName,
                UserID: UserCredentials.ID,
                EventID: SubEvent.ID,
                EventName: NameContanierInput.value,
                TimeZoneOffset: TimeZone,
                Start: SubCalStartDateInMS,
                End: SubCaEndDateInMS,
                CalStart: 0,
                CalEnd: CalEndDateInMS,
                Split: splitValue,
                ThirdPartyEventID: SubEvent.ThirdPartyEventID,
                ThirdPartyUserID: SubEvent.ThirdPartyUserID,
                ThirdPartyType: SubEvent.ThirdPartyType,
                Notes: Notes
            };

            return retValue;
        }

        function prepOnClickOfCalendarElement(SubEvent, Dom) {
            return function () {
                //event.stopPropagation();
                if (!global_UISetup.RenderOnSubEventClick.isRefListSubEventClicked)
                {
                    global_ExitManager.triggerLastExitAndPop();
                }
                DeselectAllSideBarElements();
            
            
                var AllDomsOfTheSameSubevent = $(".SameSubEvent" + SubEvent.ID);
                for (var i = 0; i < AllDomsOfTheSameSubevent.length; i++) {
                    var myDom = AllDomsOfTheSameSubevent[i]
                    $(myDom).addClass("SelectedWeekGridSubcalEvent");
                    global_previousSelectedSubCalEvent.push(myDom);
                    myDom.focus();
                }

                  var ControlPanelNameOfSubeventInfo = document.getElementById("ControlPanelNameOfSubeventInfo");
                  var ControlPanelDeadlineOfSubeventInfo = document.getElementById("ControlPanelDeadlineOfSubeventInfo");
                  var ControlPanelSubEventTimeInfo = document.getElementById("ControlPanelSubEventTimeInfo");



                  var FormatTime = function (date) {
                  var d = date;
                  var TimeHours = d.getHours();
                  var TimeMinutes = d.getMinutes();
                  var TimeMM = TimeMinutes;
                  var TimeHH = TimeHours;
                  var AMPM = 'am';
                  var day = '';
                  var month = '';
                  var date_number = d.getDate();
                  var year = d.getYear();

                  if (TimeMinutes <= 9) {
                    TimeMM = '0' +TimeMinutes
                  }
                  if (TimeHours >= 12 && TimeMinutes >= 0) {
                    if (TimeHours >= 13) {
                      TimeHH = TimeHours -12;
                  }
                    AMPM = 'pm';
                  } else if (TimeHours === 12 && TimeMinutes === 0) {
                    TimeHH = 'Noon';
                    AMPM = '';
                    TimeMM = '';
                  } else if (TimeHours === 24 && TimeMinutes === 0) {
                    TimeHH = 'Midnight';
                    AMPM = '';
                    TimeMM = '';
                  }
                  switch (d.getMonth()) {
                    case 0:
                      month = "Jan";
                      break;
                    case 1:
                      month = "Feb";
                      break;
                    case 2:
                      month = "Mar";
                      break;
                    case 3:
                      month = "Apr";
                      break;
                    case 4:
                      month = "May";
                      break;
                    case 5:
                      month = "Jun";
                      break;
                    case 6:
                      month = "Jul";
                      break;
                    case 7:
                      month = "Aug";
                      break;
                    case 8:
                      month = "Sep";
                      break;
                    case 9:
                      month = "Oct";
                      break;
                    case 10:
                      month = "Nov";
                      break;
                    case 11:
                      month = "Dec";
                      break;
                  }
                  switch (d.getDay()) {
                    case 0:
                      day = "Sunday";
                      break;
                    case 1:
                      day = "Monday";
                      break;
                    case 2:
                      day = "Tuesday";
                      break;
                    case 3:
                      day = "Wednesday";
                      break;
                    case 4:
                      day = "Thursday";
                      break;
                    case 5:
                      day = "Friday";
                      break;
                    case 6:
                      day = "Saturday";
                      break;
                  }
                  var a = { hour: TimeHH, minute: TimeMM, merid: AMPM, day: day, mon: month, date: date_number, year: year
                  }
                  return a;
                  };
                  getRefreshedData.disableDataRefresh();
                var StartDate = FormatTime(SubEvent.SubCalStartDate);
                var EndDate = FormatTime(SubEvent.SubCalEndDate);
                var Deadline = FormatTime(SubEvent.SubCalCalEventEnd);

                var yeaButton = getDomOrCreateNew("YeaToConfirmDelete");
                var nayButton = getDomOrCreateNew("NayToConfirmDelete");
                var completeButton = global_ControlPanelIconSet.getCompleteButton();
                var PauseResumeButton = global_ControlPanelIconSet.getPauseResumeButton()
                var deleteButton = global_ControlPanelIconSet.getDeleteButton();
                var DeleteMessage = getDomOrCreateNew("DeleteMessage")
                var ProcastinationButton = getDomOrCreateNew("submitProcastination");
                var ProcastinationCancelButton = getDomOrCreateNew("cancelProcastination");
                var PreviewProcrastinationButton = getDomOrCreateNew("previewProcastination");
                var ControlPanelCloseButton = global_ControlPanelIconSet.getCloseButton();
                var ProcrastinateEventModalContainer = getDomOrCreateNew("ProcrastinateEventModal");
                let NotesModal = getDomOrCreateNew("NotesModal");
                let NotesTextArea = getDomOrCreateNew("notesArea");
                let NotesCancel = getDomOrCreateNew("cancelNotes");
                let NotesSubmit = getDomOrCreateNew("submitNotes");
                var ControlPanelProcrastinateButton = global_ControlPanelIconSet.getProcrastinateButton();
                var ControlPanelLocationButton = global_ControlPanelIconSet.getLocationButton();
                var RepeatButton = global_ControlPanelIconSet.getRepeateButton();
                var NowButton = global_ControlPanelIconSet.getNowButton();
                var ModalDelete = getDomOrCreateNew("ConfirmDeleteModal")
                var ControlPanelContainer = getDomOrCreateNew("ControlPanelContainer");
                var PrimaryControlPanelContainer = getDomOrCreateNew("PrimaryControlPanelContainer");
                var MultiSelectPanel = getDomOrCreateNew("MultiSelectPanel");
                $(ControlPanelContainer).addClass("ControlPanelContainerModal");
                /*if (renderSubEventsClickEvents.BottomPanelIsOpen) {
                    closeControlPanel();
                }*/



                var InfoPanelOverLay = getDomOrCreateNew("InfoPanelOverLay")
                removeLowerBarIconSetCOntainer();
                revealControlInfoContainer()

                Object.keys(PreviewtDataDict).forEach((key) => {
                    let preview = PreviewtDataDict[key];
                    preview.hide();
                })

                closeProcrastinatePanel()


                var BasePanel = generateBasePanel();
            

                var IconSetContainer = global_ControlPanelIconSet.getIconSetContainer();
                $(ControlPanelContainer).addClass("setAsVisibilityHidden");
                setTimeout(function ()
                {
                    $(ControlPanelContainer).removeClass("setAsVisibilityHidden");
                    $(ControlPanelContainer).slideDown(500);
                }, 300)

            
                BasePanel.appendChild(InfoPanelOverLay);
                BasePanel.appendChild(IconSetContainer);
            

                PrimaryControlPanelContainer.appendChild(BasePanel);
                var EditContainerData = generateEditDoneCOntainer();
                ControlPanelContainer.appendChild(EditContainerData.Container);

                var LauchLocation = function () {
                    debugger;
                    var googleMapsURL = "https://www.google.com/maps/place/";
                    var fullURL = googleMapsURL + SubEvent.SubCalAddress
                    if (SubEvent.SubCalAddress) {
                        var win = window.open(fullURL, '_blank');
                        win.focus();
                    }
                    else {
                        $(MultiSelectPanel).removeClass("hideMultiSelectPanel");
                        MultiSelectPanel.innerHTML = "Oops tiler could not find an address :X &#x1f603;";
                        setTimeout(function () { $(MultiSelectPanel).addClass("hideMultiSelectPanel"); }, 3000);


                    }

                }

                let now = Date.now();
                let nowIsWithinSubevent = now < SubEvent.SubCalEndDate.getTime() && now >= SubEvent.SubCalStartDate.getTime();
                if (nowIsWithinSubevent) {
                    global_ControlPanelIconSet.showRepeatButton();
                    global_ControlPanelIconSet.hideNowButton();
                } else {
                    global_ControlPanelIconSet.hideRepeatButton();
                    global_ControlPanelIconSet.showNowButton();
                }

                if (!SubEvent.SubCalAddress) {
                    global_ControlPanelIconSet.hideLocationButton();
                } else {
                    ControlPanelLocationButton.onclick = LauchLocation;
                    global_ControlPanelIconSet.showLocationButton();
                }


                if (!SubEvent.isReadOnly) {
                    global_ControlPanelIconSet.showProcrastinateButton();
                    global_ControlPanelIconSet.showCompleteButton();
                    global_ControlPanelIconSet.showDeleteButton();
                } else {
                    global_ControlPanelIconSet.hideProcrastinateButton();
                    global_ControlPanelIconSet.hideCompleteButton();
                    global_ControlPanelIconSet.hideDeleteButton();
                }

                if (!SubEvent.SubCalRigid) {
                    global_ControlPanelIconSet.showProcrastinateButton();
                    global_ControlPanelIconSet.showCompleteButton();
                } else {
                    global_ControlPanelIconSet.hideProcrastinateButton();
                    if (Dictionary_OfCalendarData[SubEvent.CalendarID].Rigid) {
                        global_ControlPanelIconSet.hideCompleteButton();
                        global_ControlPanelIconSet.hideRepeatButton();
                    }
                }



                let HourInputDom = getDomOrCreateNew("procrastinateHours");
                let MinInput = getDomOrCreateNew("procrastinateMins");
                let DayInput = getDomOrCreateNew("procrastinateDays");


                function processProcrastinateButton() {
                    let isValid = isProcrastinateInputValueVallid();
                    if(isValid) {
                        $(ProcastinationButton).attr("disabled", false);
                    } else {
                        $(ProcastinationButton).attr("disabled", true);
                    }
                }

                function processPreviewButton() {
                    let isValid = isProcrastinateInputValueVallid();
                    if(isValid) {
                        $(PreviewProcrastinationButton).attr("disabled", false);
                    } else {
                        $(PreviewProcrastinationButton).attr("disabled", true);
                    }
                }

                processProcrastinateButton();
                processPreviewButton();


                $(HourInputDom).change(() => {
                    processProcrastinateButton();
                    processPreviewButton();
                })

                $(MinInput).change(() => {
                    processProcrastinateButton();
                    processPreviewButton();
                })

                $(DayInput).change(() => {
                    processProcrastinateButton();
                    processPreviewButton();
                })


                

                ProcastinationButton.onclick = function () {
                    procrastinateEvent();
                    closeProcrastinatePanel();
                }

                PreviewProcrastinationButton.onclick = function () {
                    previewProcrastinate();
                }


                ProcastinationCancelButton.onclick = closeProcrastinatePanel;

                function resetButtons() {
                    yeaButton.onclick = null;
                    nayButton.onclick = null;
                    ControlPanelProcrastinateButton.onclick = null;
                    ControlPanelCloseButton.onclick = null;
                    ControlPanelLocationButton.onclick = null;
                    RepeatButton.onclick = null;
                }

                function generateBasePanel()
                {
                    var BasePanelID = "InfoBasePanel";
                    var BasePanel = getDomOrCreateNew(BasePanelID);
                    return BasePanel;
                }

                function slideOpenProcrastinateEventModal() {
                    ProcrastinateEventModalContainer.focus();
                    $(ProcrastinateEventModalContainer).slideDown(500);
                    ProcrastinateEventModalContainer.onkeydown = ProcrastinateEventModalContainer;
                    function procrastinateContainerKeyPress(e) {
                        e.stopPropagation();
                        if (e.which == 27)//escape key press
                        {
                            closeProcrastinatePanel();
                        }
                    }
                }

                function slideOpenNotesModal() {
                    NotesModal.focus();
                    let noteText = SubEvent.Notes || ""
                    try {
                        NotesTextArea.value = decodeURI(noteText)
                    } catch (e) {
                        console.error("Failed to decode note text")
                        NotesTextArea.value = noteText
                    }
                    
                    $(NotesModal).slideDown(500);
                    NotesModal.onkeydown = notesContainerKeyPress;
                    function notesContainerKeyPress(e) {
                        e.stopPropagation();
                        if (e.which == 27)//escape key press
                        {
                            closeNotesPanel();
                        }
                    }
                }


                function closeNotesPanel(slidePanel) {
                    getDomOrCreateNew("notesArea").value = ""
                    if (slidePanel) {
                        $(NotesModal).slideUp(500);
                    } else {
                        $(NotesModal).slideUp(0);
                    }
                }

                NotesCancel.onclick = closeNotesPanel
                function submitNotes() {
                    SaveButtonClick();
                }
                NotesSubmit.onclick = submitNotes

                function generateEditDoneCOntainer()
                {
                    var SaveButton = getDomOrCreateNew("SaveButton", "button");
                    SaveButton.innerHTML = "Save"
                    var EditContainer = getDomOrCreateNew("EditCalEventContainer");
                    $(EditContainer).addClass("setAsDisplayNone");
                    EditContainer.appendChild(SaveButton);

                    var previewButon = getDomOrCreateNew("PreviewButton", "button");
                    previewButon.innerHTML = "Preview"
                    EditContainer.appendChild(previewButon);
                    var EditContainerStatus = { isRevealed: false };


                    previewButon.Dom.onclick = function () {
                        let previewDom = getDomOrCreateNew("InlineDayPreviewContainer");
                        let preview = PreviewtDataDict [SubEvent.ID];
                        if(!preview) {
                            preview = new Preview(SubEvent.ID, previewDom.Dom);
                            PreviewtDataDict[SubEvent.ID] = preview;
                        }
                        preview.editSubEvent();
                    }

                    function isSubEventPostDifferentFromSubevent(subEventPost, includeNameCheck = false) {
                        let currentSubevent = Dictionary_OfSubEvents[subEventPost.EventID];

                        let isSame = true;
                        isSame = isSame && currentSubevent.SubCalStartDate.getTime() === subEventPost.Start;
                        isSame = isSame && currentSubevent.SubCalEndDate.getTime() === subEventPost.End;
                        isSame = isSame && currentSubevent.SubCalCalEventEnd.getTime() === subEventPost.CalEnd;
                        isSame = isSame && !isNaN(subEventPost.Split) && currentSubevent.Split == subEventPost.Split;
                        isSame = isSame && currentSubevent.SubCalCalEventEnd.getTime() === subEventPost.CalEnd;
                        if (includeNameCheck) {
                            isSame = isSame && currentSubevent.SubCalCalendarName === subEventPost.EventName;
                        }

                        let isValid = true;
                        isValid = isValid && ((currentSubevent.Split == subEventPost.Split) || (subEventPost.Split > 0));

                        subEventPost.Split > 0

                        let retValue = {
                            isValid: isValid,
                            inputIsChanged: !isSame
                        }
                        return retValue;

                    }

                    function RevealEditContainer()
                    {
                        let subEventData = getSubeventUpdateData(SubEvent);
                        let dataChange = isSubEventPostDifferentFromSubevent(subEventData, true);
                        
                        if(dataChange.isValid && dataChange.inputIsChanged) {
                            if (!EditContainerStatus.isRevealed)
                            {
                                $(EditContainer).removeClass("setAsDisplayNone");
                                EditContainerStatus.isRevealed = true;
                            }
                        } else {
                            HideEditContainer();
                        }

                    }

                    
                    function HideEditContainer()
                    {
                        if (EditContainerStatus.isRevealed)
                        {
                            $(EditContainer).addClass("setAsDisplayNone");
                            EditContainerStatus.isRevealed = false;
                        }
                    }


                    HideEditContainer();
                    SaveButton.onclick = null;

                    var retValue = { SaveButton: SaveButton, Container: EditContainer, RevealContainer: RevealEditContainer, HideEditContainer: HideEditContainer };
                    return retValue;
                }


                ControlPanelProcrastinateButton.onclick = slideOpenProcrastinateEventModal;
            

                function closeControlPanel() {
                    global_ExitManager.triggerLastExitAndPop();
                }


                //function combines the close of selected reference list elements
                function CombinedCLoser()
                {
                    BindClickOfSideBarToCLick.reset();
                    TriggerClose(false);
                }


                function TriggerClose(slideClose = true) {
                    getRefreshedData.enableDataRefresh();
                    resetButtons();
                    DeselectAllSideBarElements();
                    closeModalDelete();
                    closeProcrastinatePanel(true);
                    closeNotesPanel(true)
                    deleteButton.onclick = null;
                    completeButton.onclick = null;
                    PauseResumeButton.onclick = null;
                    RepeatButton.onclick = null;
                    if (slideClose) {
                        $(ControlPanelContainer).slideUp(500);
                    } else {
                        $(ControlPanelContainer).slideUp(0);
                    }
                    
                    document.removeEventListener("keydown", containerKeyPress);
                    global_UISetup.RenderOnSubEventClick.isRefListSubEventClicked = false;
                    global_UISetup.RenderOnSubEventClick.BottomPanelIsOpen = false;
                    ActivateUserSearch.setSearchAsOn();
                    if (IconSetContainer.parentNode != null) {
                        IconSetContainer.parentNode.removeChild(IconSetContainer);
                    }
                    $(ControlPanelContainer).removeClass("ControlPanelContainerLowerBar");
                    setTimeout(function(){ $(ControlPanelContainer).removeClass("ControlPanelContainerModal")},0);
                }


                //checks if bottompannel is open. If panel is open then just reset the subevent reflist element as opposed to 
                if (!global_UISetup.RenderOnSubEventClick.isRefListSubEventClicked) {
                    global_ExitManager.addNewExit(CombinedCLoser);
                }
            
            
            
            
            

                function closeProcrastinatePanel(slidePanel) {
                    getDomOrCreateNew("procrastinateHours").value = ""
                    getDomOrCreateNew("procrastinateMins").value = ""
                    getDomOrCreateNew("procrastinateDays").value = ""
                    if (slidePanel) {
                        $(ProcrastinateEventModalContainer).slideUp(500);
                    } else {
                        $(ProcrastinateEventModalContainer).slideUp(0);
                    }
                    
                }
                closeProcrastinatePanel(false)
                closeModalDelete(false)
                closeNotesPanel(false)
                ActivateUserSearch.setSearchAsOff();
                ControlPanelCloseButton.onclick = global_ExitManager.triggerLastExitAndPop;

                function pauseEvent()
                {
                    SendMessage();
                    function SendMessage() {
                        var TimeZone = new Date().getTimezoneOffset();
                        debugger;

                        var PauseEvent = {
                            UserName: UserCredentials.UserName,
                            UserID: UserCredentials.ID,
                            EventID: SubEvent.ID,
                            TimeZoneOffset: TimeZone,
                            ThirdPartyEventID: SubEvent.ThirdPartyEventID,
                            ThirdPartyUserID: SubEvent.ThirdPartyUserID,
                            ThirdPartyType: SubEvent.ThirdPartyType
                        };
                    
                        var URL = global_refTIlerUrl + "Schedule/Event/Pause";
                        PauseEvent.TimeZone = moment.tz.guess()
                        var HandleNEwPage = new LoadingScreenControl("Tiler is Pausing your event :)");
                        HandleNEwPage.Launch();
                        preSendRequestWithLocation(PauseEvent);
                        var exit = function (data) {
                            HandleNEwPage.Hide();
                            //triggerUIUPdate();//hack alert
                            global_ExitManager.triggerLastExitAndPop();
                            //getRefreshedData();
                        }

                        $.ajax({
                            type: "POST",
                            url: URL,
                            data: PauseEvent,
                            success: function (response) {
                                exit();
                                //triggerUndoPanel("Undo Pause?");
                                //alert("alert 0-b");
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
                            triggerUIUPdate();//hack alert
                            sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
                        });
                    }
                    function triggerUIUPdate() {
                        //alert("we are deleting " + SubEvent.ID);
                        //$('#ConfirmDeleteModal').slideToggle();
                        //$('#ControlPanelContainer').slideUp(500);
                        //resetButtons();
                        global_ExitManager.triggerLastExitAndPop();
                    }

                }

                function continueEvent() {
                    SendMessage();
                    function SendMessage() {
                        var TimeZone = new Date().getTimezoneOffset();
                        debugger;

                        var ContinueEvent = {
                            UserName: UserCredentials.UserName,
                            UserID: UserCredentials.ID,
                            EventID: SubEvent.ID,
                            TimeZoneOffset: TimeZone,
                            ThirdPartyEventID: SubEvent.ThirdPartyEventID,
                            ThirdPartyUserID: SubEvent.ThirdPartyUserID,
                            ThirdPartyType: SubEvent.ThirdPartyType
                        };

                        var URL = global_refTIlerUrl + "Schedule/Event/Resume";
                        ContinueEvent.TimeZone = moment.tz.guess()
                        var HandleNEwPage = new LoadingScreenControl("Tiler resuming your event :)");
                        HandleNEwPage.Launch();

                        var exit = function (data) {
                            HandleNEwPage.Hide();
                            //triggerUIUPdate();//hack alert
                            global_ExitManager.triggerLastExitAndPop();
                            //getRefreshedData();
                        }
                        preSendRequestWithLocation(ContinueEvent);
                        $.ajax({
                            type: "POST",
                            url: URL,
                            data: ContinueEvent,
                            success: function (response) {
                                exit();
                                //triggerUndoPanel("Undo Pause?");
                                //alert("alert 0-b");
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
                            triggerUIUPdate();//hack alert
                            sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
                        });
                    }
                    function triggerUIUPdate() {
                        //alert("we are deleting " + SubEvent.ID);
                        //$('#ConfirmDeleteModal').slideToggle();
                        //$('#ControlPanelContainer').slideUp(500);
                        //resetButtons();
                        global_ExitManager.triggerLastExitAndPop();
                    }

                }

                function deleteSubevent()//triggers the yea / nay deletion of events
                {
                    DeleteMessage.innerHTML = "Sure you want to delete \"" + SubEvent.Name + "\"?"

                    yeaButton.onclick = yeaDeleteSubEvent;
                    nayButton.onclick = nayDeleteSubEvent;
                    yeaButton.focus();
                    $('#ConfirmDeleteModal').slideDown(500);
                    ModalDelete.isRevealed = true;
                }

                if (ModalDelete.isRevealed) {
                    deleteSubevent();
                }

                function yeaDeleteSubEvent()//triggers the deletion of subevent
                {
                    SendMessage();
                    function SendMessage() {
                        var TimeZone = new Date().getTimezoneOffset();
                        debugger;
                        var DeletionEvent = {
                            UserName: UserCredentials.UserName,
                            UserID: UserCredentials.ID,
                            EventID: SubEvent.ID,
                            TimeZoneOffset: TimeZone,
                            ThirdPartyEventID: SubEvent.ThirdPartyEventID,
                            ThirdPartyUserID: SubEvent.ThirdPartyUserID,
                            ThirdPartyType: SubEvent.ThirdPartyType
                    };
                        //var URL = "RootWagTap/time.top?WagCommand=6"
                        var URL = global_refTIlerUrl + "Schedule/Event";
                        DeletionEvent.TimeZone = moment.tz.guess()
                        var HandleNEwPage = new LoadingScreenControl("Tiler is Deleting your event :)");
                        HandleNEwPage.Launch();
                        preSendRequestWithLocation(DeletionEvent);
                        var exit = function (data) {
                            HandleNEwPage.Hide();
                            //triggerUIUPdate();//hack alert
                            global_ExitManager.triggerLastExitAndPop();
                            //getRefreshedData();
                        }

                        $.ajax({
                                type: "DELETE",
                                url: URL,
                                data: DeletionEvent,
                            // DO NOT SET CONTENT TYPE to json
                            // contentType: "application/json; charset=utf-8", 
                            // DataType needs to stay, otherwise the response object
                            // will be treated as a single string
                                success: function (response) {
                                    exit();
                                    triggerUndoPanel("Undo deletion?");
                                    //alert("alert 0-b");
                        },
                                error: function () {
                                var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                                var ExitAfter = { ExitNow: true, Delay: 1000
                                };
                                HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exit);
                        }
                        }).done(function (data) {
                            HandleNEwPage.Hide();
                            triggerUIUPdate();//hack alert
                            sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
                        });
                }
                    function triggerUIUPdate() {
                        global_ExitManager.triggerLastExitAndPop();
                }

            }


                function nayDeleteSubEvent()//ignores deletion of events
                {
                    closeModalDelete();
                    //resetButtons();
                }

                function procrastinateEvent() {
                    var HourInput = getDomOrCreateNew("procrastinateHours").value == "" ? 0 : getDomOrCreateNew("procrastinateHours").value;
                    var MinInput = getDomOrCreateNew("procrastinateMins").value == "" ? 0 : getDomOrCreateNew("procrastinateMins").value;
                    var DayInput = getDomOrCreateNew("procrastinateDays").value == "" ? 0 : getDomOrCreateNew("procrastinateDays").value;
                    debugger;
                    var TimeZone = new Date().getTimezoneOffset();
                    var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: SubEvent.ID, DurationDays: DayInput, DurationHours: HourInput, DurationMins: MinInput, TimeZoneOffset: TimeZone};
                    //var URL= "RootWagTap/time.top?WagCommand=2";
                    var URL = global_refTIlerUrl + "Schedule/Event/Procrastinate";
                    NowData.TimeZone = moment.tz.guess()
                    var HandleNEwPage = new LoadingScreenControl("Tiler is Postponing  :)");
                    HandleNEwPage.Launch();
                    preSendRequestWithLocation(NowData);

                    var exit = function (data) {
                        HandleNEwPage.Hide();
                        //triggerUIUPdate();//hack alert
                        global_ExitManager.triggerLastExitAndPop();
                        //getRefreshedData();
                    }
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
                                triggerUndoPanel("Undo Procrastination");
                                var myContainer = (response);
                                if (myContainer.Error.code == 0) {
                                    //exitSelectedEventScreen();
                                }
                                else {
                                    alert("error detected with marking as complete");
                                }

                            },
                            error: function () {
                            var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                            var ExitAfter = { ExitNow: true, Delay: 1000
                            };
                            HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exit);
                    }
                    }).done(function (data) {
                        HandleNEwPage.Hide();
                        triggerUIUPdate();//hack alert
                        sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
                    });

                    function triggerUIUPdate() {
                        //resetButtons();
                        global_ExitManager.triggerLastExitAndPop();
                    }
            }

                function processPreview(previewData, isUpdate = false) {
                    let PreviewModal = getDomOrCreateNew("PreviewModal")
                    
                    function renderSleepDoughnutChart (pieChartDom, data) {
                        let sleepTimeLine = data.SleepTimeline.duration
                        let sleepTimelineLabel = {
                            value: sleepTimeLine,
                            color: "#7ed629",
                            hoverColor: "#a1fb04",
                            label: 'Current Sleep Hours'
                        }
                        let dataFormated = [sleepTimelineLabel];


                        if (data.MaximumSleepTimeLine) {
                            let excessSleep = data.MaximumSleepTimeLine.duration - data.SleepTimeline.duration
                            let excessData ={
                                value: excessSleep,
                                color: "#aaaa00",
                                hoverColor: "#dddd00",
                                label: 'Extra sleep time :)'
                            };
                            dataFormated.push(excessData);
                        }

                        if (data.LostSleep) {
                            let insufficientSleep = data.LostSleep;
                            let undesiredData ={
                                value: insufficientSleep,
                                color: "#222222",
                                hoverColor: "#444444",
                                label: 'Need More :)'
                            };
                            sleepTimelineLabel.color = "#e64b19";
                            sleepTimelineLabel.hoverColor = "#f75c2a";
                            dataFormated.push(undesiredData);
                        }

                        let values = dataFormated.map(a => a.value);
                        let labels = dataFormated.map(a => a.label);
                        let backgroundColors = dataFormated.map(a => a.color);
                        let hoverBackgroundColors = dataFormated.map(a => a.hoverColor);
                        let dougnutData = {
                            labels: labels,
                            datasets: [
                                {
                                    data: values,
                                    backgroundColor: backgroundColors,
                                    hoverBackgroundColor: hoverBackgroundColors
                                }]
                        };
                        let ctx = pieChartDom.Dom;
                        ctx = ctx.getContext("2d");
                
                        var myDoughnutChart = new Chart(pieChartDom, {
                            type: 'doughnut',
                            data: dougnutData,
                            options: {
                                responsive: false,
                                legend: { 
                                    display: false,
                                    position: 'right'
                                }
                            }
                        });
                    }

                    function createSleepDom(dayData, isUndesired) {
                        let dayId = dayData.startOfDay;
                        let dayDomId = dayId + "_sleep_dayDom"
                        let retValue = getDomOrCreateNew(dayDomId)
                        $(retValue).addClass("PreviewSleepDay");
                        let dayDomContainerId = dayId + "_sleep_dayDom_container"
                        let retValueContainer = getDomOrCreateNew(dayDomContainerId);
                        $(retValueContainer).addClass("SleepDayContainer");
                        let startOfDayDate = new Date(dayData.startOfDay);


                        let weekDay = WeekDays[startOfDayDate.getDay()];
                        let weekDayId = dayId + "_" + weekDay;
                        let weekDayIdContainerName = weekDayId+"_container"

                        //weekdayTitle
                        let nameOfWeekDayDomContainer = getDomOrCreateNew(weekDayIdContainerName);
                        let weekDayIdLabel = weekDayId + "_label"
                        let weekDayLabel = getDomOrCreateNew(weekDayIdLabel, "span");
                        let WeekdayTitle = weekDay.substring(0, 3) + " " + moment(startOfDayDate).format("MM/DD")

                        weekDayLabel.innerHTML = WeekdayTitle;
                        nameOfWeekDayDomContainer.Dom.appendChild(weekDayLabel);
                        $(nameOfWeekDayDomContainer).addClass("SleepWeekDayNameContainer");
                        
                        //Chart
                        let ChartContainerId = "ChartContainer_"+dayId;
                        let ChartContainer = getDomOrCreateNew(ChartContainerId);
                        let ChartImgContainerId = "ChartImgContainer_"+dayId;
                        let ChartImgContainer = getDomOrCreateNew(ChartImgContainerId, "canvas");
                        $(ChartImgContainer).addClass("SleepDayChart");
                        ChartContainer.appendChild(ChartImgContainer);

                        // duration Content
                        let duration = dayData.SleepTimeline.duration;
                        let durationString = moment.utc(duration).format("HH:mm");
                        let durationContainerId = "durationContainerName" + dayId;
                        let durationDomContainer = getDomOrCreateNew(durationContainerId);
                        let durationDomNameId = durationContainerId+ "_label"
                        let durationDom = getDomOrCreateNew(durationDomNameId, "span");
                        durationDom.innerHTML = durationString + " Hrs";
                        $(durationDom).addClass("SleepDurationContent");
                        durationDomContainer.appendChild(durationDom);
                        $(durationDomContainer).addClass("SleepDurationContainer");


                        retValueContainer.appendChild(nameOfWeekDayDomContainer);
                        retValueContainer.appendChild(ChartContainer);
                        retValueContainer.appendChild(durationDomContainer);

                        retValue.appendChild(retValueContainer);

                        return {
                            dom: retValue,
                            renderPiechart: function () {
                                renderSleepDoughnutChart(ChartImgContainer, dayData);
                            }
                        };
                    }

                    function openPreview(previewData) {
                        if (previewData) {
                            function showSleepTimes() {
                                let sleepPreviewDomId = "SleepPreview"
                                let SleepEvaluationDomId = "SleepEvaluation"
                                function cleanUpSleepPreviewContainer() {
                                    let sleepEvaluationPreviewNode = getDomOrCreateNew(sleepPreviewDomId);
                                    while (sleepEvaluationPreviewNode.firstChild) {
                                        sleepEvaluationPreviewNode.removeChild(sleepEvaluationPreviewNode.firstChild);
                                    }
                                }

                                cleanUpSleepPreviewContainer()
                                let sleepResult = [];
                                if (previewData.after && previewData.after.sleep) {
                                    if (previewData.after.sleep.UndesiredTimeLines && previewData.after.sleep.UndesiredTimeLines.length > 0) {
                                        let timeLines = previewData.after.sleep.UndesiredTimeLines;
                                        
                                        for (let key in timeLines)
                                        {
                                            let timeline = timeLines[key];
                                            timeline.startOfDay = Number(key);
                                            let sleepDom = createSleepDom(timeline, true)
                                            sleepResult.push(sleepDom)
                                        }
                                    } else {
                                        let timeLines = previewData.after.sleep.SleepTimeLines;
                                        for (let key in timeLines) {
                                            let timeline = timeLines[key];
                                            timeline.startOfDay = Number(key)
                                            let sleepDom = createSleepDom(timeline, false);
                                            sleepResult.push(sleepDom);
                                        }
                                    }
                                }
                                let sleepEvaluationPreviewNode = getDomOrCreateNew(sleepPreviewDomId);
                                let sleepEvaluationDom = getDomOrCreateNew(SleepEvaluationDomId);
                                if(!sleepEvaluationDom.status) {
                                    $(sleepEvaluationDom).addClass("SleepEvaluation");
                                    sleepEvaluationPreviewNode.appendChild(sleepEvaluationDom);
                                }

                                sleepResult.forEach((sleepResponse) => {
                                    sleepEvaluationDom.appendChild(sleepResponse.dom)
                                    sleepResponse.renderPiechart()
                                })
                                
                            }
                            showSleepTimes();
                        }
                        $(PreviewModal).removeClass("inActive");
                        $(PreviewModal).addClass("active");
                        $(PreviewModal).removeClass("setAsDisplayNone");
                    }

                    function closePreview() {
                        $(PreviewModal).removeClass("active");
                        $(PreviewModal).addClass("inActive");
                    }
                    debugger
                    let closePreviewButton = getDomOrCreateNew("closePreview");
                    closePreviewButton.innerHTML = "Close"
                    closePreviewButton.onclick = closePreview;
                    openPreview(previewData);
                }

                
                
                function previewProcrastinate() {
                    let previewDom = getDomOrCreateNew("InlineDayPreviewContainer");
                    let preview = PreviewtDataDict [SubEvent.ID];
                    if(!preview) {
                        preview = new Preview(SubEvent.ID, previewDom.Dom);
                        PreviewtDataDict [SubEvent.ID] = preview;
                    }
                    preview.procrastinateEvent();
                }

                previewProcrastinate = buildFunctionSubscription(previewProcrastinate)


                function markAsComplete() {
                    SendMessage();
                    function SendMessage() {
                        var TimeZone = new Date().getTimezoneOffset();
                        var Url;
                        //Url="RootWagTap/time.top?WagCommand=7";
                        Url = global_refTIlerUrl + "Schedule/Event/Complete";
                        var HandleNEwPage = new LoadingScreenControl("Tiler is updating your schedule ...");
                        HandleNEwPage.Launch();

                        var MarkAsCompleteData = {
                            UserName: UserCredentials.UserName,
                            UserID: UserCredentials.ID,
                            EventID: SubEvent.ID,
                            TimeZoneOffset: TimeZone,
                            ThirdPartyEventID: SubEvent.ThirdPartyEventID,
                            ThirdPartyUserID: SubEvent.ThirdPartyUserID,
                            ThirdPartyType: SubEvent.ThirdPartyType
                        };
                        MarkAsCompleteData.TimeZone = moment.tz.guess()
                        preSendRequestWithLocation(MarkAsCompleteData);

                        var exit = function (data) {
                            HandleNEwPage.Hide();
                            //triggerUIUPdate();//hack alert
                            global_ExitManager.triggerLastExitAndPop();
                            //getRefreshedData();
                        }
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
                                    triggerUndoPanel("Undo Completion");
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
                                var ExitAfter = { ExitNow: true, Delay: 1000
                                };
                                HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exit);
                                    //InitializeHomePage();


                        }

                        }).done(function (data) {
                            debugger;
                            HandleNEwPage.Hide();
                            triggerUIUPdate();//hack alert
                            sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
                        });
                    }
                        function triggerUIUPdate() {
                            global_ExitManager.triggerLastExitAndPop();
                    }

                }

                function repeatSubEventRequest() {
                    let TimeZone = new Date().getTimezoneOffset();
                    let Url = global_refTIlerUrl + "SubCalendarEvent/Repeat";
                    let HandleNEwPage = new LoadingScreenControl("Tiler is configuring the repetition ...");
                    HandleNEwPage.Launch();

                    var RepetitionData = {
                        UserName: UserCredentials.UserName,
                        UserID: UserCredentials.ID,
                        EventID: SubEvent.ID,
                        TimeZoneOffset: TimeZone,
                        ThirdPartyEventID: SubEvent.ThirdPartyEventID,
                        ThirdPartyUserID: SubEvent.ThirdPartyUserID,
                        ThirdPartyType: SubEvent.ThirdPartyType
                    };
                    RepetitionData.TimeZone = moment.tz.guess()
                    preSendRequestWithLocation(RepetitionData);

                    var exit = function (data) {
                        HandleNEwPage.Hide();
                        global_ExitManager.triggerLastExitAndPop();
                    }

                    $.ajax({
                        type: "POST",
                        url: Url,
                        data: RepetitionData,
                        // DO NOT SET CONTENT TYPE to json
                        // contentType: "application/json; charset=utf-8", 
                        // DataType needs to stay, otherwise the response object
                        // will be treated as a single string
                        //dataType: "json",
                        success: function (response) {
                            triggerUndoPanel("Undo Repeat");
                            var myContainer = (response);
                            if (myContainer.Error.code == 0) {
                                HandleNEwPage.Hide();
                            }
                            else {
                                let customError = myContainer.Error;
                                let message = customError.Message || "Ooops Tiler is having updating your schedule."
                                var ExitAfter = {
                                    ExitNow: true, Delay: 3000
                                };
                                HandleNEwPage.UpdateMessage(message, ExitAfter, exit);
                            }

                        },
                        error: function (err) {
                            var myError = err;
                            var step = "err";
                            var NewMessage = "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                            var ExitAfter = {
                                ExitNow: true, Delay: 1000
                            };
                            HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exit);
                        }

                    }).done(function (data) {
                        debugger;
                        triggerUIUPdate();//hack alert
                        sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
                    });

                    function triggerUIUPdate() {
                        global_ExitManager.triggerLastExitAndPop();
                    }
                }


                function setSubEventAsNow() {

                }
                function closeModalDelete(slidePanel = true) {
                    DeleteMessage.innerHTML = "Sure you want to delete ?"
                    if (slidePanel) {
                        $('#ConfirmDeleteModal').slideUp(500);
                    } else {
                        $('#ConfirmDeleteModal').slideUp(0);
                    }
                    ModalDelete.isRevealed = false;
                }
                deleteButton.onclick = deleteSubevent;
                completeButton.onclick = markAsComplete;
                RepeatButton.onclick = repeatSubEventRequest;

                let afterNowButtonCliclCallBack = function() {
                    global_ExitManager.triggerLastExitAndPop();
                    getRefreshedData();
                }
                NowButton.onclick = genFunctionCallForNow(SubEvent.ID, afterNowButtonCliclCallBack);
                if (SubEvent.isPaused) {
                    global_eventIsPaused = true;
                    global_ControlPanelIconSet.switchToResumeButton();
                    PauseResumeButton.onclick = continueEvent;
                    $(ControlPanelCloseButton).addClass("setAsDisplayNone");
                    global_ControlPanelIconSet.ShowPausePauseResumeButton();
                }
                else {
                    global_ControlPanelIconSet.switchToPauseButton();
                    PauseResumeButton.onclick = pauseEvent;
                    $(ControlPanelCloseButton).addClass("setAsDisplayNone");
                    if((SubEvent.isPauseAble)&&(!global_eventIsPaused)) 
                    {
                        global_ControlPanelIconSet.ShowPausePauseResumeButton();
                    }
                    else
                    {
                        global_ControlPanelIconSet.HidePausePauseResumeButton();
                    }
                }

                var ControlPanelContainer = getDomOrCreateNew("ControlPanelContainer");
                ControlPanelContainer.focus();

                function containerKeyPress(e) {
                    if (e.which == 27)//escape key press
                    {
                        return;//closeControlPanel();
                    }

                    if ((e.which == 8) || (e.which == 46))//bkspc/delete key pressed
                    {
                        deleteSubevent();
                    }
                }
                document.removeEventListener("keydown", containerKeyPress);//this is here just to avooid duplicate addition of the same keypress event
                document.addEventListener("keydown", containerKeyPress);

                global_UISetup.RenderOnSubEventClick.isRefListSubEventClicked = true;
                global_UISetup.RenderOnSubEventClick.BottomPanelIsOpen = true;

                function stopPropagationOfKeyDown(e) {
                    if (e.which == 27)
                    {
                        return;
                    }
                    e.stopPropagation();
                }
                var NameContanierInput = getDomOrCreateNew("NameInputBox", "input");
                $(NameContanierInput).off();
                NameContanierInput.value = SubEvent.Name;
                NameContanierInput.onkeydown = stopPropagationOfKeyDown;

                $(NameContanierInput).on("input", EditContainerData.RevealContainer)

                var SubEventStartTime = getDomOrCreateNew("StartTimeInput", "input");
                $(SubEventStartTime).off();
                var SubEventStartBinder = BindTimePicker(SubEventStartTime)
                SubEventStartBinder.on('input', EditContainerData.RevealContainer);
                SubEventStartBinder.on('changeTime', EditContainerData.RevealContainer);
            
                SubEventStartTime.value = SubEvent.SubCalStartDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
                var AmDash0 = getDomOrCreateNew("AmDash0", "span");
                AmDash0.innerHTML = ' &mdash; '
                var SubEventEndTime = getDomOrCreateNew("EndTimeInput", "input");
                $(SubEventEndTime).off();
                var SubEventEndBinder = BindTimePicker(SubEventEndTime)
                SubEventEndBinder.on('input', EditContainerData.RevealContainer);
                SubEventEndBinder.on('changeTime', EditContainerData.RevealContainer);
            
                SubEventEndTime.value= SubEvent.SubCalEndDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })

                SubEventStartTime.onkeydown = stopPropagationOfKeyDown;
                SubEventEndTime.onkeydown = stopPropagationOfKeyDown;


                var SubEventStartDate = getDomOrCreateNew("SubEventStartDateInput", "input");
                $(SubEventStartDate).off("input");
                $(SubEventStartDate).off("changeDate");
                SubEventStartDate.value = SubEvent.SubCalStartDate.toLocaleDateString();//position of this insertion matters. It has to be before the call to "BindDatePicker" or else the input value will get reset at each entry
                var SubEventStartDateBinder = BindDatePicker(SubEventStartDate, "D M d, yyyy");
                SubEventStartDateBinder.datepicker("setDate", new Date(SubEventStartDate.value));
                SubEventStartDateBinder.on('input', EditContainerData.RevealContainer);
                SubEventStartDateBinder.on('changeDate', EditContainerData.RevealContainer);
            

            
                var AmDash1 = getDomOrCreateNew("AmDash1", "span");
                AmDash1.innerHTML = ' &mdash; '
                var SubEventEndDate = getDomOrCreateNew("SubEventEndDateInput", "input");
                $(SubEventEndDate).off("input");
                $(SubEventEndDate).off("changeDate");
                SubEventEndDate.value = SubEvent.SubCalEndDate.toLocaleDateString();//position of this insertion matters. It has to be before the call to "BindDatePicker"
                var SubEventEndDateBinder = BindDatePicker(SubEventEndDate, "D M d, yyyy");
                SubEventEndDateBinder.datepicker("setDate", new Date(SubEventEndDate.value));
                SubEventEndDateBinder.on('input', EditContainerData.RevealContainer)
                SubEventEndDateBinder.on('changeDate', EditContainerData.RevealContainer);
            
                if (SubEvent.isReadOnly) {
                    NameContanierInput.Dom.disabled = true;
                    SubEventStartTime.Dom.disabled = true;
                    SubEventEndTime.Dom.disabled = true;
                    SubEventStartDate.Dom.disabled = true;
                    SubEventEndDate.Dom.disabled = true;
                } else {
                    NameContanierInput.Dom.disabled = false;
                    SubEventStartTime.Dom.disabled = false;
                    SubEventEndTime.Dom.disabled = false;
                    SubEventStartDate.Dom.disabled = false;
                    SubEventEndDate.Dom.disabled = false;
                }
            



                SubEventStartDate.onkeydown = stopPropagationOfKeyDown;
                SubEventEndDate.onkeydown = stopPropagationOfKeyDown;

                var SubCalStartInfo = getDomOrCreateNew("SubCalStartInfo");
                var SubCalEndInfo = getDomOrCreateNew("SubCalEndInfo");
                $(SubCalEndInfo).addClass( "TimeDateContainer")
                $(SubCalStartInfo).addClass("TimeDateContainer");
            


                var CalEndTime = getDomOrCreateNew("CalEndTime", "input");
                CalEndTime.onkeydown = stopPropagationOfKeyDown;
                CalEndTime.onkeydown = stopPropagationOfKeyDown;

                $(CalEndTime).off("input");
                $(CalEndTime).off("changeTime");
                CalEndTime.value = SubEvent.SubCalCalEventEnd.toLocaleTimeString();//position of this insertion matters. It has to be before the call to "BindTimePicker"
                var CalEventEndTimeBinder = BindTimePicker(CalEndTime);
                CalEventEndTimeBinder.on('input', EditContainerData.RevealContainer)
                CalEventEndTimeBinder.on('changeTime', EditContainerData.RevealContainer);


                //var CalStartDate = getDomOrCreateNew("CalStartDate", "input");
                var CalEndDate = getDomOrCreateNew("CalEndDate", "input");
                //CalStartDate.onkeydown = stopPropagationOfKeyDown;
                CalEndDate.onkeydown = stopPropagationOfKeyDown;
            
                $(CalEndDate).off("input");
                $(CalEndDate).off("changeDate");
                CalEndDate.value = SubEvent.SubCalCalEventEnd.toLocaleDateString();//position of this insertion matters. It has to be before the call to "BindDatePicker"
                var CalEventEndDateBinder = BindDatePicker(CalEndDate, "D M d, yyyy");
                CalEventEndDateBinder.datepicker("setDate", new Date(CalEndDate.value));
                CalEventEndDateBinder.on('input', EditContainerData.RevealContainer)
                CalEventEndDateBinder.on('changeDate', EditContainerData.RevealContainer);


                var calendarEventData = getDomOrCreateNew("CalEndDate", "input");
                ControlPanelNameOfSubeventInfo.appendChild(NameContanierInput);


                function renderNotesUIData(subEvent)
                {
                    let editNotesbutton = getDomOrCreateNew("editNotes", "button");
                    editNotesbutton.innerHTML = SubEvent.Notes ? 'Edit Notes' : 'Add Notes'
                    editNotesbutton.onclick = slideOpenNotesModal
                    let retValue = {
                        button: editNotesbutton ,
                        getNotesValue: function () {

                        },
                    }
                    return retValue;
                }
            
                function extraOptionsData()
                {
                    var splitAndNoteContainer = getDomOrCreateNew("SplitCountAndNoteContainer");
                    var ContainerForExtraOptions = getDomOrCreateNew("ExtraOptionsContainer")
                    ContainerForExtraOptions.appendChild(splitAndNoteContainer)
                    let splitInputBox = getDomOrCreateNew("InputSplitCount", "input");
                    let splitInputBoxContainer = getDomOrCreateNew("InputSplitCountContainer");
                    var splitInputBoxLabel = getDomOrCreateNew("splitInputBoxLabel", "label");
                    splitInputBoxLabel.innerHTML = "Splits"

                    if (!SubEvent.isThirdParty) {
                        $(ContainerForExtraOptions.Dom).removeClass("setAsDisplayNone");
                        $(splitAndNoteContainer).addClass("SubEventInformationContainer");
                        if (!Dictionary_OfCalendarData[SubEvent.CalendarID].Rigid) {
                            splitInputBox.oninput = EditContainerData.RevealContainer;
                            splitInputBox.setAttribute("type", "Number");
                            splitInputBox.onkeydown = stopPropagationOfKeyDown;
                            splitInputBox.value = Dictionary_OfCalendarData[SubEvent.CalendarID].TotalNumberOfEvents;

                            splitInputBoxContainer.appendChild(splitInputBoxLabel);
                            splitInputBoxContainer.appendChild(splitInputBox);
                            splitAndNoteContainer.appendChild(splitInputBoxContainer);
                            let CompletionMap = getDomOrCreateNew("CompletionContainer");
                            let CompletionMapDom = generateCompletionMap(SubEvent)
                            CompletionMap.appendChild(CompletionMapDom);
                            ContainerForExtraOptions.appendChild(CompletionMap)
                            $(splitInputBoxContainer.Dom).removeClass("setAsDisplayNone");
                            $(CompletionMap.Dom).removeClass("setAsDisplayNone");
                        } else {
                            splitInputBox.Dom.value = 1;
                            splitInputBoxContainer.appendChild(splitInputBox);
                            splitAndNoteContainer.appendChild(splitInputBoxContainer);
                            let CompletionMap = getDomOrCreateNew("CompletionContainer");
                            $(splitInputBoxContainer.Dom).addClass("setAsDisplayNone");
                            $(CompletionMap.Dom).addClass("setAsDisplayNone");
                        }

                        let renderNoteResult = renderNotesUIData(null);
                        splitAndNoteContainer.appendChild(renderNoteResult.button)
                    } else {
                        splitInputBox.Dom.value = 1;
                        splitInputBoxContainer.appendChild(splitInputBoxLabel);
                        splitInputBoxContainer.appendChild(splitInputBox);
                        splitAndNoteContainer.appendChild(splitInputBoxContainer);
                        $(splitInputBoxContainer.Dom).addClass("setAsDisplayNone");
                        $(ContainerForExtraOptions.Dom).addClass("setAsDisplayNone");
                    }
                    return ContainerForExtraOptions;
                }

                
                function handleSuggestedDeadline() {
                    let suggestedDedalineContainerId = "suggested-deadline-container"
                    let suggestedDedalineContainerDom = getDomOrCreateNew(suggestedDedalineContainerId);
                    let suggestedDeadline = SubEvent.SuggestedDeadline || SubEvent.LastSuggestedDeadline
                    if ( suggestedDeadline && suggestedDeadline > 0) {
                        let suggestedTime = new Date(suggestedDeadline)
                        let onSuggestionClick = () => {
                            CalEndTime.value = moment(suggestedTime, "MM-DD-YYYY").format("hh:mma");
                            CalEventEndDateBinder.datepicker("setDate", suggestedTime);
                            EditContainerData.RevealContainer()
                        };
                        let suggestedDedalineButtonId = "suggested-deadline-container-button"
                        let suggestedDedalineContainerButtonDom = getDomOrCreateNew(suggestedDedalineButtonId);
                        suggestedDedalineContainerDom.appendChild(suggestedDedalineContainerButtonDom);
                        suggestedDedalineContainerButtonDom.addEventListener("click", onSuggestionClick);
                        suggestedDedalineContainerButtonDom.Dom.innerHTML = "Try Suggested Deadline?";
                        let suggestedDateString = moment(suggestedTime, "MM-DD-YYYY").format("ddd MMM DD, YYYY");
                        suggestedDedalineContainerButtonDom.setAttribute('title', suggestedDateString);
                        $(suggestedDedalineContainerDom).removeClass("setAsDisplayNone");
                    } else {
                        $(suggestedDedalineContainerDom).addClass("setAsDisplayNone");
                    }

                    return suggestedDedalineContainerDom;
                }

                SubCalStartInfo.appendChild(SubEventStartTime);
                SubCalStartInfo.appendChild(SubEventStartDate);

                SubCalEndInfo.appendChild(SubEventEndTime);
                SubCalEndInfo.appendChild(SubEventEndDate);


                ControlPanelSubEventTimeInfo.appendChild(SubCalStartInfo);
                ControlPanelSubEventTimeInfo.appendChild(SubCalEndInfo);
            

                var ControlPanelDeadlineOfSubevent=getDomOrCreateNew("ControlPanelDeadlineOfSubevent")
                ControlPanelDeadlineOfSubeventInfo.appendChild(CalEndTime);
                ControlPanelDeadlineOfSubeventInfo.appendChild(CalEndDate);
                let suggestDeadlineButton = handleSuggestedDeadline()
                ControlPanelDeadlineOfSubeventInfo.appendChild(suggestDeadlineButton);
                

                

                var InfoCOntainer = getDomOrCreateNew("InfoContainer")
                let optionData = extraOptionsData();
                InfoCOntainer.appendChild(optionData);
                if (Dictionary_OfCalendarData[SubEvent.CalendarID].Rigid)
                {
                    $(ControlPanelDeadlineOfSubevent).addClass("setAsDisplayNone");
                }
                else
                {
                    $(ControlPanelDeadlineOfSubevent).removeClass("setAsDisplayNone");
                }



                function SaveButtonClick()
                {
                    debugger;
                    SubEventEndTime
                    let Url = global_refTIlerUrl + "SubCalendarEvent/Update";
                    let HandleNEwPage = new LoadingScreenControl("Tiler is updating your schedule ...");
                    HandleNEwPage.Launch();
                    let SaveData = getSubeventUpdateData(SubEvent);
                    SaveData.TimeZone = moment.tz.guess();
                    preSendRequestWithLocation(SaveData);

                    var exit= function (data) {
                        HandleNEwPage.Hide();
                        //triggerUIUPdate();//hack alert
                        global_ExitManager.triggerLastExitAndPop();
                        sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
                    }
                    $.ajax({
                        type: "POST",
                        url: Url,
                        data: SaveData,
                        // DO NOT SET CONTENT TYPE to json
                        // contentType: "application/json; charset=utf-8", 
                        // DataType needs to stay, otherwise the response object
                        // will be treated as a single string
                        //dataType: "json",
                        success: function (response) {
                            triggerUndoPanel("Undo Change to event on Tiler");
                            var myContainer = (response);
                            if (myContainer.Error.code == 0) {
                                //exitSelectedEventScreen();
                            }
                            else {
                                var NewMessage = myContainer.Error && myContainer.Error.code && myContainer.Error.Message ? myContainer.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                                var ExitAfter = {
                                    ExitNow: true, Delay: 5000
                                };
                                HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exit);
                            }

                        },
                        error: function (err) {
                            var myError = err;
                            var step = "err";
                            var NewMessage = err.Error && err.Error.code && err.Error.Message ? err.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                            var ExitAfter = {
                                ExitNow: true, Delay: 1000
                            };
                            HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, exit);
                        }

                    }).done(exit);

                }

                EditContainerData.SaveButton.onclick = SaveButtonClick;

            

                //ControlPanelDeadlineOfSubeventInfo.value = Deadline.hour + ' ' + Deadline.minute + ' ' + Deadline.merid + ' // ' + Deadline.day + ', ' + Deadline.mon + ' ' + Deadline.date;
                //ControlPanelSubEventTimeInfo.value = EndDate.hour + ' ' + StartDate.minute + ' ' + StartDate.merid + ' &mdash; ' + EndDate.hour + ' ' + EndDate.minute + ' ' + EndDate.merid;
                var SubEventName = SubEvent.Name;
                $(document).keyup(function (e) {
                  if (e == 46) {
                    deleteEvent();
                };
                });

            }
        }


        function StopPullingData() {
    clearTimeout(global_ClearRefreshDataInterval);
    }


        function PauseDataPolling(PauseByMs) {
    if (PauseByMs ==undefined) {
        PauseByMs = 300000;
        }

    getRefreshedData.isEnabled = false;
    }


        function PopulateYourself(RangeData) {
    var StartDate = new Date(Number(this.SubCalStartDate));
    var EndDate = new Date(Number(this.SubCalEndDate));
    this.RangeCounter = new Array();
    this.Dom = { DomA: getDomOrCreateNew("EventDOMA" + this.ID), DomB: getDomOrCreateNew("EventDOMB" + this.ID)
        };


    var i =0;
    for (; i <RangeData.length; i++) {
        if ((StartDate >= new Date(Number(RangeData[i].Start))) && (StartDate <= new Date(Number(RangeData[i].End)))) {
            this.RangeCounter.push(RangeData[i]);
    }

        if ((EndDate >= new Date(Number(RangeData[i].Start))) && (EndDate <= new Date(Number(RangeData[i].End)))) {
            this.RangeCounter.push(RangeData[i]);
    }
        }



    if (this.RangeCounter[0] != this.RangeCounter[1]) {

    }
    else {

        }

    }


        function PopulateInputContainerOption(SubEvent) {

    }

        function UpdateDeadline() {
    var UpdateDeadlineID = "UpdateDeadline";
    var UpdateDeadline = getDomOrCreateNew(UpdateDeadlineID, "button");
    }


        function DeleteCurrentRepetition() {

    }

        function DeleteCurrentRepetition() {

    }

        function getRangeofSchedule() {
    var CalStartRange = null;
    var CalEndRange = null;
    if (TotalSubEventList.length > 0) {
        CalStartRange = new Date(TotalSubEventList[0].SubCalStartDate);
        CalEndRange = new Date(TotalSubEventList[TotalSubEventList.length -1].SubCalEndDate);
        }
    var earliestDayIndex = CalStartRange.getDay();
    var earliestMonth = CalStartRange.getMonth();
    var earliestYear = CalStartRange.getFullYear();

    var latestDayIndex = CalEndRange.getDay();
    var latestMonth = CalStartRange.getMonth();
    var latestYear = CalStartRange.getFullYear();

    var TimeSpanInDayMS = earliestDayIndex *OneDayInMs;

    var CalStartDay = Number(CalStartRange) - Number(TimeSpanInDayMS);
    CalStartDay = new Date(CalStartDay);
    CalStartDay.setHours(0, 0, 0, 0);


    var EndTimeSpanInDayMS = ((7 - latestDayIndex) % 7) * OneDayInMs;

    var CalEndDay =Number(CalEndRange) + Number(EndTimeSpanInDayMS);
    CalEndDay = new Date(CalEndDay);
    CalEndDay.setHours(0, 0, 0, 0);

    return { Start: CalStartDay, End: CalEndDay
        }

    }


function genDivForEachWeek(RangeOfWeek, AllRanges)//generates each week container giving the range of the week
{
    var DayIndex = 0;
    var widthPercent = 100/7;
    var refDate = new Date(RangeOfWeek.Start);
    var WeekID = Number(RangeOfWeek.Start) + "_" + Number(RangeOfWeek.End)
    var WeekRange = getDomOrCreateNew(WeekID);
    var WeekGridRenderPlaneID = WeekID + "WeekGridRenderPlane"
    var RenderPlane = getDomOrCreateNew(WeekGridRenderPlaneID);
    $(RenderPlane.Dom).addClass("WeekGridRenderPlane");

    var SubEventRenderPlaneID = WeekID + "SubEventRenderPlane"
    var SubEventRenderPlane = getDomOrCreateNew(SubEventRenderPlaneID);
    var NameOfWeekDayRenderPlaneID = WeekID + "NameOfWeekDayRenderPlane"
    var NameOfWeekDayRenderPlane = getDomOrCreateNew(NameOfWeekDayRenderPlaneID);
    $(NameOfWeekDayRenderPlane.Dom).addClass("NameOfWeekDayRenderPlane");
    

    let loadingBarWrapperId = WeekID + "-LoadingPanel-wrapper"
    let loadingBarWrapper = getDomOrCreateNew(loadingBarWrapperId);
    $(loadingBarWrapper).addClass("LoadingPanel-wrapper");
    let loadingBar = new LoadingBar(weeklyScheduleLoadingBar);
    loadingBar.embed(loadingBarWrapper);

    $(SubEventRenderPlane.Dom).addClass("SubEventRenderPlane");
    RenderPlane.appendChild(NameOfWeekDayRenderPlane);
    RenderPlane.appendChild(SubEventRenderPlane);
    RenderPlane.appendChild(loadingBarWrapper);
    var prev;

    

    var StartOfDay = new Date(RangeOfWeek.Start);
    let NextStartOfDay = StartOfDay;
    var TodayStart = new Date(Date.now());
    TodayStart.setHours(0);
    TodayStart.setMinutes(0);
    TodayStart.setSeconds(0);
    TodayStart.setMilliseconds(0);
    var TodayInMS= TodayStart.getTime();
    if (!WeekRange.status) {
        var DaysOfWeekDoms = new Array();
        
        //WeekRange.Dom.appendChild(RenderPlane.Dom);
        WeekDays.forEach(
            function (DayOfWeek) {
                var myDay = generateDayContainer();
                StartOfDay = NextStartOfDay;
                myDay.widtPct = widthPercent;
                myDay.Start = new Date(StartOfDay);//set start of day property
                NextStartOfDay = new Date(StartOfDay);// this gets updated for the next loop
                NextStartOfDay.setDate(StartOfDay.getDate() + 1);
                let endTime = new Date(myDay.Start);
                endTime.setDate(endTime.getDate() + 1);

                myDay.End = new Date(endTime.getTime() - 1);
                var isToday = myDay.Start.getTime() == TodayInMS;
                var currDate = new Date(Number(refDate.getTime()));
                currDate.setDate(currDate.getDate() + DayIndex);

                
                prev = currDate;
                var Month = currDate.getMonth() +1;
                var Day = currDate.getDate();
                myDay.NameOfDayContainer.Dom.innerHTML = "<p class='NameWeekDay'>" + DayOfWeek.substring(0, 2) + "</p>" + "<p class='dateOfDay'>" + myDay.Start.getDate() + "</p>";
                var MoreOptions = getDomOrCreateNew("DayMoreOptions" + myDay.DayID)
                $(MoreOptions).addClass("DayMoreOptions")

                var DayMoreOptionsUnderlinesTop = getDomOrCreateNew("DayMoreOptionsUnderlinesTop" + myDay.DayID)
                $(DayMoreOptionsUnderlinesTop).addClass("DayMoreOptionsUnderlinesTop")
                $(DayMoreOptionsUnderlinesTop).addClass("ThreeDot")
                var DayMoreOptionsUnderlinesBottom = getDomOrCreateNew("DayMoreOptionsUnderlinesBottom" + myDay.DayID)
                $(DayMoreOptionsUnderlinesBottom).addClass("DayMoreOptionsUnderlinesBottom")
                $(DayMoreOptionsUnderlinesBottom).addClass("ThreeDot")
                var DayMoreOptionsUnderlinesMiddle = getDomOrCreateNew("DayMoreOptionsUnderlinesMiddle" + myDay.DayID)
                $(DayMoreOptionsUnderlinesMiddle).addClass("DayMoreOptionsUnderlinesMiddle")
                $(DayMoreOptionsUnderlinesMiddle).addClass("ThreeDot")


                MoreOptions.appendChild(DayMoreOptionsUnderlinesTop)
                MoreOptions.appendChild(DayMoreOptionsUnderlinesBottom)
                MoreOptions.appendChild(DayMoreOptionsUnderlinesMiddle)
                myDay.NameOfDayContainer.Dom.appendChild(MoreOptions);
                NameOfWeekDayRenderPlane.appendChild(myDay.NameOfDayContainer);


                MoreOptions.onclick =MoreOptionsClick;

                function MoreOptionsClick()
                {
                    //return;
                    global_ExitManager.triggerLastExitAndPop();
                    myDay.RevealMoreOptions();
                    MoreOptions.onclick = MoreOptionsOutsideClick;
                    $(SubEventRenderPlane).addClass("HideSubEventRenderPlane");
                    global_ExitManager.addNewExit(MoreOptionsOutsideClick);
                    GetDeletedEvents(myDay, ProcessDeletedEvents);
                    function ProcessDeletedEvents(DeletedData)
                    {
                        //debugger;
                        function generateListDom(SubEventData)
                        {
                            var myID = generateListDom.Count++;
                            var InActiveElementContainer = getDomOrCreateNew("InActiveElementContainer" + myID);
                            $(InActiveElementContainer).addClass("InActiveElementContainer");
                            var InActiveElementNameContainer = getDomOrCreateNew("InActiveElementNameContainer" + myID);
                            $(InActiveElementNameContainer).addClass("InActiveElementNameContainer");
                            InActiveElementNameContainer.setAttribute("title", SubEventData.Name);
                            var InActiveElementNameSpan = getDomOrCreateNew("InActiveElementNameSpan" + myID,"span");
                            $(InActiveElementNameSpan).addClass("InActiveElementNameSpan");
                            

                            InActiveElementNameSpan.innerHTML = SubEventData.Name;
                            InActiveElementNameContainer.appendChild(InActiveElementNameSpan)
                            var InActiveElementIconContainer = getDomOrCreateNew("InActiveElementIconContainer" + myID);
                            $(InActiveElementIconContainer).addClass("InActiveElementIconContainer");
                            if (SubEventData.isComplete) {
                                $(InActiveElementNameSpan).addClass("CompletedInactive");
                                $(InActiveElementNameSpan).addClass("Checkmark");
                            }

                            if (!SubEventData.isEnabled) {
                                $(InActiveElementNameSpan).addClass("DeletedInactive");
                                $(InActiveElementNameSpan).addClass("Crossmark");
                            }

                            InActiveElementContainer.appendChild(InActiveElementIconContainer);
                            InActiveElementContainer.appendChild(InActiveElementNameContainer);
                            myDay.MoreInfoPanel.appendChild(InActiveElementContainer);
                        }
                        generateListDom.Count = 0;
                        DeletedData.forEach(generateListDom);
                        if(!DeletedData.length)
                        {

                            var EmptySpan = getDomOrCreateNew("EmptySpan" + generateListDom.Count, "span");
                            EmptySpan.innerHTML ="...Empty..."
                            //$(InActiveElementNameSpan).addClass("InActiveElementNameSpan");
                            myDay.MoreInfoPanel.appendChild(EmptySpan)
                            setTimeout(function () { EmptySpan.innerHTML = "" }, 3000)
                        }
                        
                    }
                }

                function MoreOptionsOutsideClick()
                {
                    $(SubEventRenderPlane).removeClass("HideSubEventRenderPlane");
                    myDay.UnRevealMoreOptions();
                    MoreOptions.onclick = MoreOptionsClick;
                }


                if (isToday)
                {
                    var PreviousDay = $(".CurrentDay");
                    PreviousDay.removeClass("CurrentDay");
                    $(myDay.NameOfDayContainer).addClass("CurrentDay");
                }
                myDay.SubEventsCollection = {};
                BindClickTOStartOfDay(myDay);
                myDay.UISpecs = {
                };

                myDay.WeekRenderPlane = SubEventRenderPlane;
                $(myDay.Parent.Dom).addClass(DayOfWeek + "DayContainer");
                SubEventRenderPlane.appendChild(myDay.Parent.Dom);
                //WeekRange.Dom.appendChild(myDay.Parent.Dom);
                myDay.LeftPercent = (DayIndex * widthPercent);
                myDay.RightPercent = myDay.LeftPercent + widthPercent;

                DayIndex += 1;
                DaysOfWeekDoms.push(myDay);
            }
        );
        SubEventRenderPlane.Start = RangeOfWeek.Start;
        SubEventRenderPlane.End = RangeOfWeek.End;
        BindAddNewEventToClick(SubEventRenderPlane);
        WeekRange.DaysOfWeek = DaysOfWeekDoms;
        var Index =AllRanges.length//gets index, because it gets the index bewfore the push
        AllRanges.push(WeekRange);
        AllRanges[Index].Start = RangeOfWeek.Start;
        AllRanges[Index].End = RangeOfWeek.End;
        AllRanges[Index].UISpecs = {};
        AllRanges[Index].SubEventsCollection = {};
    }
    WeekRange.renderPlane = SubEventRenderPlane;


    WeekRange.Dom.appendChild(RenderPlane.Dom);

    var RetValue = { NameOfWeek: NameOfWeekDayRenderPlane, WeekTwentyFourHourGrid: RenderPlane }
    return RetValue;
}

        //function creates Bind a click event to the render plane to enable addition of new events
        function BindAddNewEventToClick(Week) {
    var RenderPlaneDom = Week.Dom;
    $(RenderPlaneDom).click(function (e) {
        //debugger;
        var posX = $(this).offset().left
        var posY = $(this).offset().top;
        var left = e.pageX -posX;
        var top = e.pageY -posY;
        var height = $(RenderPlaneDom).height();
        var width = $("body").width() - e.pageX ;
        //debugger;

        //alert(width);
        //getDomOrCreateNew("CurrentWeekContainer")
        generateModal(left, top, height, width, Week.Start, this);
        e.stopPropagation();
    });
    }
        function BindClickTOStartOfDay(myDay) {
            function CallBackFunc() {
        refreshIframe(myDay.Start, myDay.End);
        }

    $(myDay.NameOfDayContainer.Dom).click(CallBackFunc);
    }


function GlobaPauseResumeButtonManager(events) {
    var _this = this;
    _this.SubEvents = events;
    var currentTime = new Date();
    var currentIndex = -1;
    this.timeOutData = -1;
    var isPausedIndex = -1;
    var isPauseAbleIndex = -1;
    var nextEventIndex = 0;
    var buttonId = "GlobalPauseResumeButton"
    HidePauseResumeButton();
    function getPausedEventOrNextPossibleEvent(subEvents)
    {
        var i = -1;
        var currentSubEvent = null;
        var nextEvent = null;
        isPausedIndex = -1;
        isPauseAbleIndex = -1;
        nextEventIndex = 0;
        if(isArray (subEvents)){
            for (i = 0; i < subEvents.length; i++)
            {
                var subEvent = subEvents[i];
                if (subEvent.isPaused)
                {
                    isPausedIndex = i;
                    break;
                }
                if (subEvent.isPauseAble)
                {
                    isPauseAbleIndex = i;
                    //break;
                }

                if (subEvent.SubCalEndDate > currentTime)
                {
                    nextEventIndex = i;
                    //break;
                }

                if (subEvent.SubCalStartDate > currentTime) {
                    break;
                }
            }


            if ((isPausedIndex < subEvents.length) && (isPausedIndex > -1))
            {
                currentSubEvent = subEvents[isPausedIndex];
                SwitchToResume(currentSubEvent.ID);
                _this.currentIndex = isPausedIndex;
                return;
            }

            if ((isPauseAbleIndex < subEvents.length) && (isPauseAbleIndex > -1)) {
                currentSubEvent = subEvents[isPauseAbleIndex];
                SwitchToPause(currentSubEvent.ID);
                _this.currentIndex = isPauseAbleIndex;
                prepTimeOutForNextEvent(_this.currentIndex);
                return;
            }

            if ((nextEventIndex < subEvents.length) && (nextEventIndex > -1)) {
                currentSubEvent = subEvents[nextEventIndex];
                HidePauseResumeButton();
                _this.currentIndex = nextEventIndex;
                _this.currentIndex -=1;
                prepTimeOutForNextEvent(_this.currentIndex);
                return;
            }
        }

                
    }

    function prepTimeOutForNextEvent(currentIndex) {
        var nextEventIndex = currentIndex + 1;
        var nextSubEvent;
        var currentSubEvent = _this.SubEvents[currentIndex]
        if(nextEventIndex < _this.SubEvents.length) {
            nextSubEvent = _this.SubEvents[nextEventIndex]
            var TimeSpan = new Date(nextSubEvent.SubCalStartDate).getTime()  - new Date().getTime()
            function timeOutCallBack() {
                HidePauseResumeButton();
                SwitchToPause(nextSubEvent.ID);
                _this.currentIndex = nextEventIndex;
                prepTimeOutForNextEvent(nextEventIndex);
            }
            if(TimeSpan > 0){
                _this.timeOutData = setTimeout(timeOutCallBack, TimeSpan);
            }
            else{
                _this.timeOutData = setTimeout(timeOutCallBack,0);
            }
        }
        //hides pause button if next event begins after the end of the current event
        if(!!currentSubEvent) {
            var hidePauseButton = false;
            if(!!nextSubEvent) 
            {
                if(new Date(currentSubEvent.SubCalEndDate).getTime() < new Date(nextSubEvent.SubCalStartDate).getTime())
                {
                    hidePauseButton = true;
                }
            }

            if(hidePauseButton) 
            {
                var HideTimeSpan  = new Date(currentSubEvent.SubCalEndDate).getTime()  - new Date().getTime()
                setTimeout(HidePauseResumeButton, HideTimeSpan);
            }
        }
    }

    function SwitchToResume(eventId)
    {
        var pauseResumeButton = getDomOrCreateNew(buttonId)
        $(pauseResumeButton).addClass("ControlPanelResumePanelButton");
        $(pauseResumeButton).removeClass("ControlPanelPausePanelButton");
        ShowPauseResumeButton();
        var SubEvent = Dictionary_OfSubEvents[eventId];
        if (SubEvent) {
            pauseResumeButton.setAttribute("Title", "Resume \"" + SubEvent.Name + "\"");
            pauseResumeButton.onclick = function () {
                continueEvent(SubEvent);
            }
        }
        
    }

    function SwitchToPause(eventId)
    {
        var SubEvent = Dictionary_OfSubEvents[eventId];
        if (SubEvent.isPauseAble) {
            var pauseResumeButton = getDomOrCreateNew(buttonId)
            $(pauseResumeButton).addClass("ControlPanelPausePanelButton");
            $(pauseResumeButton).removeClass("ControlPanelResumePanelButton");
            ShowPauseResumeButton();
            pauseResumeButton.setAttribute("Title", "Pause \"" + SubEvent.Name + "\"");
            var SubEvent = Dictionary_OfSubEvents[eventId];
            pauseResumeButton.onclick = function () {
                pauseEvent(SubEvent);
            }
        }
    }

    function HidePauseResumeButton() {
        var pauseResumeButton = getDomOrCreateNew(buttonId)
        pauseResumeButton.onclick = null;
        $(pauseResumeButton).addClass("setAsDisplayNone");

    }

    function ShowPauseResumeButton() {
        var pauseResumeButton = getDomOrCreateNew(buttonId)
        $(pauseResumeButton).removeClass("setAsDisplayNone");
    }

    function pauseEvent(SubEvent) {
        SendMessage();
        function SendMessage() {
            var TimeZone = new Date().getTimezoneOffset();
            debugger;

            var PauseEvent = {
                UserName: UserCredentials.UserName,
                UserID: UserCredentials.ID,
                EventID: SubEvent.ID,
                TimeZoneOffset: TimeZone,
                ThirdPartyEventID: SubEvent.ThirdPartyEventID,
                ThirdPartyUserID: SubEvent.ThirdPartyUserID,
                ThirdPartyType: SubEvent.ThirdPartyType
            };
            PauseEvent.TimeZone = moment.tz.guess()
            var URL = global_refTIlerUrl + "Schedule/Event/Pause";
            var HandleNEwPage = new LoadingScreenControl("Tiler is Pausing your event :)");
            HandleNEwPage.Launch();
            preSendRequestWithLocation(PauseEvent);


            var exit = function (data) {
                HandleNEwPage.Hide();
                //triggerUIUPdate();//hack alert
                global_ExitManager.triggerLastExitAndPop();
                //getRefreshedData();
            }

            $.ajax({
                type: "POST",
                url: URL,
                data: PauseEvent,
                success: function (response) {
                    exit();
                    //triggerUndoPanel("Undo Pause?");
                    //alert("alert 0-b");
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
                triggerUIUPdate();//hack alert
                sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
            });
        }
        function triggerUIUPdate() {
            global_ExitManager.triggerLastExitAndPop();
        }
    }

    function continueEvent(SubEvent) {
        SendMessage();
        function SendMessage() {
            var TimeZone = new Date().getTimezoneOffset();
            debugger;

            var ContinueEvent = {
                UserName: UserCredentials.UserName,
                UserID: UserCredentials.ID,
                EventID: SubEvent.ID,
                TimeZoneOffset: TimeZone,
                ThirdPartyEventID: SubEvent.ThirdPartyEventID,
                ThirdPartyUserID: SubEvent.ThirdPartyUserID,
                ThirdPartyType: SubEvent.ThirdPartyType
            };
            ContinueEvent.TimeZone = moment.tz.guess()
            var URL = global_refTIlerUrl + "Schedule/Event/Resume";
            var HandleNEwPage = new LoadingScreenControl("Tiler resuming your event :)");
            HandleNEwPage.Launch();
            preSendRequestWithLocation(ContinueEvent);

            var exit = function (data) {
                HandleNEwPage.Hide();
                //triggerUIUPdate();//hack alert
                global_ExitManager.triggerLastExitAndPop();
                //getRefreshedData();
            }

            $.ajax({
                type: "POST",
                url: URL,
                data: ContinueEvent,
                success: function (response) {
                    exit();
                    //triggerUndoPanel("Undo Pause?");
                    //alert("alert 0-b");
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
                triggerUIUPdate();//hack alert
                sendPostScheduleEditAnalysisUpdate({CallBackSuccess: getRefreshedData});;
            });
        }
        function triggerUIUPdate() {
            //alert("we are deleting " + SubEvent.ID);
            //$('#ConfirmDeleteModal').slideToggle();
            //$('#ControlPanelContainer').slideUp(500);
            //resetButtons();
            global_ExitManager.triggerLastExitAndPop();
        }

    }

    this.updateEventList = function (events)
    {
        if (_this.timeOutData != -1) {
            clearTimeout(_this.timeOutData);
        }
        _this.SubEvents = events;
        getPausedEventOrNextPossibleEvent(_this.SubEvents);
    }

    var startTImeOutIds = [];
    var endTImeOutIds = [];

    this.processPauseData = function (pauseData)
    {
        var currentTimeInMs = new Date().getTime();
        //If an event is currently paused then just show resume button and don't handle other pauseable events
        if (pauseData.pausedEvent != null)
        {
            SwitchToResume(pauseData.pausedEvent.EventId);
        }
        else
        {
            var i = 0;
            for (; i < startTImeOutIds.length; i++)
            {
                clearTimeout(startTImeOutIds[i])
            }

            for (i = 0; i < endTImeOutIds.length; i++) {
                clearTimeout(endTImeOutIds[i])
            }

            //function creates the event function that will make a call to the switch of the global pause resume button
            function prepSpanCallback(subEvent)
            {
                function retValue ()
                {
                    function HideGlobalPausePauseButton()
                    {
                        if (prepSpanCallback.pauseId === subEvent.ID) {
                            HidePauseResumeButton()
                        }
                    }

                    SwitchToPause(subEvent.ID);
                    prepSpanCallback.pauseId = subEvent.ID;
                    var Span = subEvent.PauseEnd - currentTimeInMs ;
                    var TimeOutID = setTimeout(HideGlobalPausePauseButton, Span);
                    endTImeOutIds.push(TimeOutID);
                }
                return retValue 
            }

            for (i=0; i < pauseData.subEvents.length; i++) {
                var subEvent = pauseData.subEvents[i];
                var Span = subEvent.PauseStart - currentTimeInMs;
                var endSpan = subEvent.PauseEnd - currentTimeInMs;
                var eventId = subEvent.ID;
                var TimeOutID;

                if (Span < 0) {
                    if (endSpan > 0) {
                        TimeOutID = setTimeout(prepSpanCallback(subEvent))
                        startTImeOutIds.push(TimeOutID);
                    }
                }
                else {
                    TimeOutID = setTimeout(prepSpanCallback(subEvent), Span)
                    startTImeOutIds.push(TimeOutID);
                }
                
            }
        }
    }

    getPausedEventOrNextPossibleEvent(_this.SubEvents);
}

