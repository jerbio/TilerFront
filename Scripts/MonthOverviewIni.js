"use strict"

var global_DictionaryOfSubEvents = {};
var global_RemovedElemnts = {};
var global_WeekGrid;
var global_DayHeight;
var global_WeekWidth;
var global_DayTop;
var global_RangeMultiplier = 5;//range for number of weeks to be specified for calculation
var global_CurrentRange;
var global_ClearRefreshDataInterval = 0;
var global_ColorAugmentation = 0;
var refreshCounter = 1000000;
var global_refreshDataInterval = 60000;
var global_multiSelect;
var global_ControlPanelIconSet = new IconSet();
var global_GoToDay;
$(global_ControlPanelIconSet.getIconSetContainer()).addClass("ControlPanelIconSetContainer");

$(document).ready(function () {
    $(document).tooltip({ track: true });
    $('body').hide();
    InitializeMonthlyOverview();
    MenuManger();
});


function MenuManger()
{
    var TilerManager = getDomOrCreateNew("TilerManager");
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
        }//,200)
    }

    function unRevealMenuContainer()
    {
        MenuContainer.style.display = "none";
        MenuContainer.style.width = 0;
        MenuContainer.style.left = "-60px"
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

function AddCloseButoon(Container, LeftPosition)
{
    var TilerCloseButtonID = "TilerCloseButton"
    var TilerCloseButton = getDomOrCreateNew("TilerCloseButton");
    $(TilerCloseButton.Dom).attr('tabindex', 0)
    $(TilerCloseButton).removeClass("LeftCloseButton");
    $(TilerCloseButton).removeClass("RightCloseButton");
    $(TilerCloseButton).removeClass("setAsDisplayNone");

    if(LeftPosition)
    {
        $(TilerCloseButton).addClass("LeftCloseButton");
    }
    else
    {
        $(TilerCloseButton).addClass("RightCloseButton");
    }
    

    var LeftCloseBar = getDomOrCreateNew("LeftCloseBar");
    var RightCloseBar = getDomOrCreateNew("RightCloseBar");

    TilerCloseButton.onclick = function (e) {
        e.stopPropagation();
        global_ExitManager.triggerLastExitAndPop();
    };
    TilerCloseButton.appendChild(LeftCloseBar);
    TilerCloseButton.appendChild(RightCloseBar);
    

    Container.appendChild(TilerCloseButton);
}

AddCloseButoon.HideClosButton=function()
{
    var TilerCloseButton = getDomOrCreateNew("TilerCloseButton");
    $(TilerCloseButton).addClass("setAsDisplayNone");
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
    var ProcatinationButton = getDomOrCreateNew("submitProcatination");
    var ProcatinationCancelButton = getDomOrCreateNew("cancelProcatination");
    var ControlPanelCloseButton = RevealControlPanelSection.IconSet.getCloseButton();
    
    var ProcrastinateEventModalContainer = getDomOrCreateNew("ProcrastinateEventModal");
    var ControlPanelProcrastinateButton = RevealControlPanelSection.IconSet.getProcrastinateButton();
    $(ControlPanelProcrastinateButton).addClass("setAsDisplayNone");
    $(RevealControlPanelSection.IconSet.getLocationButton()).addClass("setAsDisplayNone");
    var ModalDelete = getDomOrCreateNew("ConfirmDeleteModal")
    var MultiSelectPanel = getDomOrCreateNew("MultiSelectPanel")
    var ControlPanelContainer = getDomOrCreateNew("ControlPanelContainer");
    var IconSetContainer = RevealControlPanelSection.IconSet.getIconSetContainer();
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
    renderSubEventsClickEvents.BottomPanelIsOpen = true;
    ControlPanelContainer.appendChild(IconSetContainer);

    $(global_ControlPanelIconSet.getIconSetContainer())

    $('#ControlPanelContainer').slideDown(500);

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
        renderSubEventsClickEvents.BottomPanelIsOpen = false;
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
            var AllIds = Object.keys(SelectedEvents).join(',');

            var DeletionEvent = {
                UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: AllIds, TimeZoneOffset: TimeZone
            };
            //var URL = "RootWagTap/time.top?WagCommand=6"
            var URL = global_refTIlerUrl + "Schedule/Events";
            var HandleNEwPage = new LoadingScreenControl("Tiler is Deleting your event :)");
            HandleNEwPage.Launch();

            var exitSendMessage = function (data) {
                HandleNEwPage.Hide();
                //triggerUIUPdate();//hack alert
                global_ExitManager.triggerLastExitAndPop();
                getRefreshedData();
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
                getRefreshedData();
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
            var AllIds = Object.keys(SelectedEvents).join(',');
            var MarkAsCompleteData = {
                UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: AllIds, TimeZoneOffset: TimeZone
            };

            var exit = function (data) {
                HandleNEwPage.Hide();
                //triggerUIUPdate();//hack alert
                global_ExitManager.triggerLastExitAndPop();
                getRefreshedData();
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
                getRefreshedData();
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
    //document.onkeydown = containerKeyPress
   
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

    this.getCloseButton = function ()
    {
        return CloseIcon;
    }

    this.getLocationButton = function () {
        return LocationIcon;
    }


    this.getDeleteButton = function () {
        return DeleteIcon;
    }

    this.getCompleteButton = function () {
        return CompleteIcon;
    }

    this.getProcrastinateButton = function () {
        return ProcrastinateIcon;
    }

    this.getIconSetContainer = function () {
        return IconSetContainer;
    }

    IconSetContainer.appendChild(LocationIcon)
    IconSetContainer.appendChild(ProcrastinateIcon)
    IconSetContainer.appendChild(DeleteIcon)
    IconSetContainer.appendChild(CompleteIcon)
    IconSetContainer.appendChild(CloseIcon)
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
    $(DayContextContainer).addClass("DayContextContainer");
    DayContainer.appendChild(DayContextContainer);
    //DayContainer
    DayContainer.onmouseover = onMouseIn;
    DayContainer.onmouseout = onMouseOut;
    
    $(DayContainer.Dom).addClass("DayContainer");
    var NameOfDayContainer = getDomOrCreateNew("NameOfDayContainer" + myID);
    NameOfDayContainer.onclick = function (e) { e.stopPropagation(); };
    var DayTimeContainer = getDomOrCreateNew("DayTimeContainer" + myID);
    $(DayTimeContainer).addClass("setAsDisplayNone");
    $(NameOfDayContainer.Dom).addClass("NameOfDayContainer");
    DayContainer.Dom.appendChild(NameOfDayContainer.Dom);

    DayContextContainer.Dom.appendChild(DayTimeContainer.Dom);
    DayContextContainer.Dom.appendChild(SubEventListContainer.Dom);
    //DayContainer.Dom.appendChild(DayTimeContainer.Dom);
    $(DayTimeContainer.Dom).addClass("DayTimeContainer");
    var NumberOfShaders = 24;
    var TotalTopElement = 0;
    var ConstTopIncrement = 2;

    for (; NumberOfShaders > 0; --NumberOfShaders) {
        var shadeContainer = getDomOrCreateNew("shadeContainer" + myID + "" + NumberOfShaders);
        DayTimeContainer.Dom.appendChild(shadeContainer.Dom);
        shadeContainer.Dom.style.top = TotalTopElement + "%";
        TotalTopElement += 4.1667;
        if (NumberOfShaders % 2) {
            $(shadeContainer.Dom).addClass("DarkerShade");
        }
        else {
            $(shadeContainer.Dom).addClass("LighterShade");
        }

        $(shadeContainer.Dom).addClass("shadeContainer");
    }
    function onMouseIn()
    {
        //debugger;
        setTimeout(function ()
        {
            $(DayTimeContainer).removeClass("setAsDisplayNone");
        })
    }

    function onMouseOut()
    {
        setTimeout(function () {
            //debugger;
            //return;
            $(DayTimeContainer).addClass("setAsDisplayNone");
        })
        
    }

    ///*
    
    //*/

    var EventDayContainer = getDomOrCreateNew("EventDayContainer" + generateDayContainer.id);
    //DayContainer.onmou = function () { alert("Hey Jay") };
    
    $(EventDayContainer.Dom).addClass("EventDayContainer");
    
    DayTimeContainer.Dom.appendChild(EventDayContainer.Dom);
    return { renderPlane: SubEventListContainer, FullDayContext: DayContextContainer, Parent: DayContainer, EventDayContainer: EventDayContainer, NameOfDayContainer: NameOfDayContainer }
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

function InitializeMonthlyOverview()
{
    BindAddButton();
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

function populateMonth(refDate)
{
    if (refDate == null)
    {
        refDate = Date.now();
    }
    refDate = new Date(refDate);
    global_WeekGrid = InitiateGrid(refDate);
    /*$('#MonthGrid').fullCalendar({
        dayClick: function (obj)
        {
            var myVar = obj;
            var Y_M_D = myVar._i.split("-");
            Y_M_D[0] = parseInt(Y_M_D[0]);
            Y_M_D[1] = parseInt(Y_M_D[1])-1;
            Y_M_D[2] = parseInt(Y_M_D[2]);
            var SelectedDate = new Date(Y_M_D[0], Y_M_D[1], Y_M_D[2]);
            if ((SelectedDate >= global_CurrentRange.Start) && (SelectedDate < global_CurrentRange.End))
            {
                scrollToDay(SelectedDate);
                return;
            }


            global_WeekGrid = InitiateGrid(SelectedDate);
            getRefreshedData(global_WeekGrid);
        }
    })*/

    LaunchMonthTicker(refDate);
    getRefreshedData();
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
        $("#CurrentWeekContainer").animate({ scrollLeft: WidthInPixels }, 1000);
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
    //$(RightContainer.Dom).addClass("RightTimeOfDayContainer");



    var RangeData = PopulateUI(encasingDOm, refDate);
    var TimeBarContainer = document.getElementById("CurrentWeekContainer");
    $(LeftContainer.Dom).addClass("DayOfTime");
    $(RightContainer.Dom).addClass("DayOfTime");
    encasingDOm.appendChild(RightContainer.Dom);
    encasingDOm.appendChild(LeftContainer.Dom);
    
    return RangeData;
}





function getRefreshedData()//RangeData)
{
    //setTimeout(refreshIframe,200);
    if (--refreshCounter < 0)//debugging counter. THis allows us to set a max number of refreshes before stopping calls to backend
    {
        return;
    }
    StopPullingData();
    monthViewResetData();
    var DataHolder = { Data: "" };
    if (getRefreshedData.isEnabled)
    {
        
        PopulateTotalSubEvents(DataHolder, global_WeekGrid);
        
    }
        global_ClearRefreshDataInterval = setTimeout(getRefreshedData, global_refreshDataInterval);
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
    
    function PopulateTotalSubEvents(DataHolder, RangeData)
    {
        ///Gets the data from tiler back end. Also sucks out the subcalendar events


        //var myurl = "RootWagTap/time.top?WagCommand=0";
        var myurl = global_refTIlerUrl + "Schedule";
        var TimeZone = new Date().getTimezoneOffset();
        if (new Date().dst())
        {
            //TimeZone += 60;
        }
        getRefreshedData.disableDataRefresh();
        var PostData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, StartRange: RangeData.Start.getTime(), EndRange: RangeData.End.getTime(), TimeZoneOffset: TimeZone };

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
        }).done(function ()
        {
            //alert("done generating");
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
            //NewData = JSON.parse(NewData);
            NewData = NewData.Content;
            StructuralizeNewData(NewData)
            DataContainer.Data = NewData;
        }

    }




    function StructuralizeNewData(NewData)
    {
        if (NewData != "") {
            TotalSubEventList = new Array();
            generateNonRepeatEvents(NewData.Schedule.NonRepeatCalendarEvent);
            generateRepeatEvents(NewData.Schedule.RepeatCalendarEvent);
            global_RemovedElemnts = global_DictionaryOfSubEvents;
            global_DictionaryOfSubEvents = {};
        }
        else
        {
            console.log("Empty Data");
        }
        
    }


    function PopulateMonthGrid(NewData, RangeData)
    {
        //populates the grid with the provided grid data
        /*var encasingDOm = document.getElementById("FullWeekContainer");
        var RangeData = PopulateUI(encasingDOm);*/
        //genFunctionForSelectCalendarRange(RangeData, 1)();
        /*
        NewData.Schedule.RepeatCalendarEvent.forEach(function (RepeatCalendarEvent) {
            RepeatCalendarEvent.RepeatCalendarEvents.forEach(
                function (repeatCalEvent) {
                    repeatCalEvent.AllSubCalEvents.forEach(
                        function (subEvent) {
                            getMyPositionFromRange(subEvent, RangeData);
                        })
                })
        })
        //*/

        
        TotalSubEventList.forEach(
            function (subEvent)
            {
                delete global_RemovedElemnts[subEvent.ID];
                getMyPositionFromRange(subEvent, RangeData);
            
                global_DictionaryOfSubEvents[subEvent.ID] = subEvent;
                global_DictionaryOfSubEvents[subEvent.ID].AllCallBacks = new Array();
            });

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


        RangeData.forEach(
            function (WeekRange)
            {
                WeekRange.DaysOfWeek.forEach(triggerUIUPdate);
                
            });

        RangeData.forEach(
            function (WeekRange) {
                WeekRange.DaysOfWeek.forEach(renderUIChanges);

            });
        return;
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

    function triggerUIUPdate(DayOfWeek)
    {
        var verfyDate = new Date(2014, 5, 15, 0, 0, 0, 0);
        var a = 0;
    

        var IntersectingArrayData = new Array();
        var Now = new Date();
        for (var ID in DayOfWeek.UISpecs)
        {
            var TopPixels = ((DayOfWeek.UISpecs[ID].css.top / 100) * global_DayHeight) + global_DayTop;
            if (DayOfWeek.UISpecs[ID].Enabled)
            {
                
                var ListElementContainer = getDomOrCreateNew("SubEventReference" + ID);
                $(ListElementContainer.Dom).addClass("ListElement");
                global_DictionaryOfSubEvents[ID].gridDoms.push(ListElementContainer.Dom)//Adds the List element as list of candidates to be deleted
                DayOfWeek.UISpecs[ID].refrenceListElement =ListElementContainer;
                var TimeDataPerListElement = getDomOrCreateNew("SubEventReferenceTime" + ID);
                var NameDataPerListElement = getDomOrCreateNew("SubEventReferenceName" + ID);
                
                $(ListElementContainer.Dom).addClass("selectedDayElements");
               // $(ListElementContainer.Dom).removeClass("ListElements");
                //$(obj).addClass("ListElements");
                NameDataPerListElement.Dom.innerHTML = global_DictionaryOfSubEvents[ID].Name;
                TimeDataPerListElement.Dom.innerHTML = getTimeStringFromDate(global_DictionaryOfSubEvents[ID].SubCalStartDate) + " - " + getTimeStringFromDate(global_DictionaryOfSubEvents[ID].SubCalEndDate);
                $(NameDataPerListElement.Dom).addClass("SubEventReferenceName");
                $(TimeDataPerListElement.Dom).addClass("SubEventReferenceTime");
                

                

                var ListElementDataContentContainer = getDomOrCreateNew("ListElementDataContentContainer" + ID);
                $(ListElementDataContentContainer.Dom).addClass("ListElementDataContentContainer");
                ListElementDataContentContainer.Dom.appendChild(NameDataPerListElement.Dom);
                ListElementDataContentContainer.Dom.appendChild(TimeDataPerListElement.Dom);
                
                global_DictionaryOfSubEvents[ID].gridDoms.push(ListElementDataContentContainer.Dom)
                

                var EventLockContainer = getDomOrCreateNew("EventLockContainer" + ID);
                $(EventLockContainer.Dom).addClass("EventLockContainer");
                var myBool = (global_DictionaryOfSubEvents[ID].SubCalRigid)
                var EventLockImgContainer = getDomOrCreateNew("EventLockImgContainer" + ID);
                $(EventLockImgContainer.Dom).addClass("EventLockImgContainer");
                if (myBool) {
                    $(EventLockImgContainer.Dom).addClass("LockedIcon");
                }

                EventLockContainer.Dom.appendChild(EventLockImgContainer.Dom)
                

                ListElementContainer.Dom.appendChild(EventLockContainer.Dom);
                ListElementContainer.Dom.appendChild(ListElementDataContentContainer.Dom);
                
                DayOfWeek.UISpecs[ID].DataElement=ListElementDataContentContainer
                global_DictionaryOfSubEvents[ID].ListRefElement = ListElementContainer;


                IntersectingArrayData.push({ Start: DayOfWeek.UISpecs[ID].Start, Data: DayOfWeek.UISpecs[ID], ID: ID, Count: 0, top: TopPixels,refSubEvent:ListElementContainer })
            }
        }

        //AllEvents.sort(function (a, b) { return (a.SubCalStartDate) - (b.SubCalStartDate) });
        //debugger;
        IntersectingArrayData.sort(function (a, b) { return (a.Start) - (b.Start) });
        var MinPercent = ((1/24)* 100);//40 derived from min pixel height.
        
        var myIndex=0;
        
        for(var i=0;i<IntersectingArrayData.length;i++)
        {
            var ID = IntersectingArrayData[i].ID;
            if (DayOfWeek.UISpecs[ID].Enabled)
            {
                var widthSubtraction = 0;
                var LeftPercent = 0;
                DayOfWeek.UISpecs[ID].Dom.Active = true;
                DayOfWeek.UISpecs[ID].Enabled = false;
                DayOfWeek.UISpecs[ID].Dom.Enabled = false;
                
                var HeightPx=(DayOfWeek.UISpecs[ID].css.height/100)*global_DayHeight;
                if (DayOfWeek.UISpecs[ID].css.height < MinPercent)
                {
                    //HeightPx=40;
                }
                var TopPixels=IntersectingArrayData[i].top;
                var EndPixelTop = TopPixels + HeightPx;

                if (DoIInterSect(EndPixelTop, i, IntersectingArrayData))
                {
                    widthSubtraction += 10;
                }
                LeftPercent = IntersectingArrayData[i].Count*17;

                widthSubtraction += IntersectingArrayData[i].Count * 10;
                if (widthSubtraction >= 90)
                {
                    widthSubtraction = 90;
                }

                if (LeftPercent > 90)
                {
                    LeftPercent = 90;
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
                if ((Now >= DayOfWeek.Start) && (Now < DayOfWeek.End))
                {
                    $(DayOfWeek.UISpecs[ID].refrenceListElement.Dom).addClass("selectedDayElements");
                }
                DayOfWeek.renderPlane.Dom.appendChild(DayOfWeek.UISpecs[ID].refrenceListElement.Dom)
                $(DayOfWeek.UISpecs[ID].Dom).show();
                setTimeout(prepUiSlideFunc(DayOfWeek, ID, IntersectingArrayData, i), 200 * ++a);
                DayOfWeek.maxIndex = global_ListOfDayCounter;

            }
            else
            {
                
            
            }
        }
        return;
    }


    function PositionIconSet(DayContainer, SubEventDom)
    {
        //debugger;
        DayContainer = DayContainer.Parent;
        //var IconSetContainer = global_ControlPanelIconSet.getIconSetContainer();
        var ControlPanelContainer = getDomOrCreateNew("ControlPanelContainer");
        var documentWidth = $(document).width();
        var buffer = 20;
        var widthOfIconSet = $(ControlPanelContainer).width()
        var heightOfIconSet = $(ControlPanelContainer).height()
        var widthOfDayContainer = $(DayContainer).width();
        var widthOfListSubEvent = $(SubEventDom).width();
        var leftOfDayContainer = $(SubEventDom).offset().left;
        var RightBorder = leftOfDayContainer + widthOfListSubEvent + widthOfIconSet;
        var topOfSubEvent = $(SubEventDom).offset().top;
        var LeftOffset=0
        if (RightBorder > documentWidth) {
            LeftOffset = leftOfDayContainer - widthOfIconSet// - buffer
        }
        else
        {
            LeftOffset = leftOfDayContainer + widthOfListSubEvent + buffer;
        }
        ControlPanelContainer.style.top = topOfSubEvent + "px";
        ControlPanelContainer.style.left = LeftOffset + "px";
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
            if (!renderSubEventsClickEvents.isRefListSubEventClicked) {
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

    function DoIInterSect(End,Index,AllElements)
    {
        var retValue = false;
        for (var i = Index+1; i < AllElements.length; i++)
        {
            if (AllElements[i].top.toFixed(2) < End.toFixed(2))
            {
                ++AllElements[i].Count;
                retValue = true;
            }
        }

        return retValue;
    }


    function HighlightSubEvent(Dom)
    {
        $(Dom).addClass("selectedElements");
    }

function dehighlightSubEvent(Dom)
{
    $(Dom).removeClass("selectedElements");
}
function prepUiSlideFunc(DayOfWeek, ID, MyArray, Index)
{
    return function () {
        //DayOfWeek.UISpecs[ID].Dom.style.left = DayOfWeek.LeftPercent + DayOfWeek.widtPct + "%";
        /*
        * DayOfWeek.UISpecs[ID].Dom is the left bar element
        * DayOfWeek.UISpecs[ID].refrenceListElement is the list element
        */

        //DayOfWeek.UISpecs[ID].Dom.style.left = "100%";
        DayOfWeek.UISpecs[ID].Dom.style.left = DayOfWeek.RightPercent+"%"
        DayOfWeek.UISpecs[ID].Dom.style.height = DayOfWeek.UISpecs[ID].css.height + "%";
        //DayOfWeek.UISpecs[ID].Dom.style.minHeight = (global_DayHeight * (1 / 24)) + "px";//1/24 because we want the minimum to be the size of an hour

        DayOfWeek.UISpecs[ID].Dom.style.marginLeft = (- (DayOfWeek.UISpecs[ID].css.left + 18) + "px");
        //DayOfWeek.UISpecs[ID].Dom.style.width = DayOfWeek.UISpecs[ID].css.width + "%";
        DayOfWeek.UISpecs[ID].Dom.style.top = DayOfWeek.UISpecs[ID].css.top + "%";
        if (DayOfWeek.UISpecs[ID].IDindex ==0) {
            //triggers change to List elements UI elements
                //DayOfWeek.UISpecs[ID].refrenceListElement.Dom.style.top = (Index * 20) + "px";
                //DayOfWeek.UISpecs[ID].refrenceListElement.Dom.style.marginTop = (20) + "px";
                $(DayOfWeek.UISpecs[ID].refrenceListElement.Dom).removeClass("FullColorAugmentation");
            //DayOfWeek.UISpecs[ID].refrenceListElement.Dom.style.left = DayOfWeek.LeftPercent + "%";
                DayOfWeek.UISpecs[ID].refrenceListElement.Dom.style.left = 0;
            if (global_DictionaryOfSubEvents[ID].ColorSelection > 0) {
                //$(DayOfWeek.UISpecs[ID].refrenceListElement.Dom).addClass(global_AllColorClasses[global_DictionaryOfSubEvents[ID].ColorSelection].cssClass);
                $(DayOfWeek.UISpecs[ID].DataElement.Dom).addClass(global_AllColorClasses[global_DictionaryOfSubEvents[ID].ColorSelection].cssClass);
        }

    }
        var BindToThis = BindClickOfSideBarToCLick(MyArray, DayOfWeek.UISpecs[ID], Index, DayOfWeek.UISpecs[ID].Dom,ID);
        var Range = global_DictionaryOfSubEvents[ID].Day;
        DayOfWeek.UISpecs[ID].refrenceListElement.Dom.onmouseover = function () {
            //debugger;
            Range.Parent.onmouseover();
        }

        DayOfWeek.UISpecs[ID].refrenceListElement.Dom.onmouseout = function () {
            //debugger;
            Range.Parent.onmouseout();
        }


        //if statement ensures that only elements with IDIndex generate the function. This ensures that the selected element shows on the left side of the page.
        if ((global_DictionaryOfSubEvents[ID].Bind == null) && (DayOfWeek.UISpecs[ID].IDindex == 0)) {
            global_DictionaryOfSubEvents[ID].Bind = BindToThis;
    }
        /*
        function triggerClick(e) {
            if(e!=null)
            {
                e.stopPropagation();
            }
            global_DictionaryOfSubEvents[ID].Bind()
        };
        //$(DayOfWeek.UISpecs[ID].Dom).click(triggerClick);
        (DayOfWeek.UISpecs[ID].Dom).onclick=(triggerClick);

        $(DayOfWeek.UISpecs[ID].refrenceListElement.Dom).click
        (
            function (e) 
            { 
                e.stopPropagation();
                triggerClick();
                //$(DayOfWeek.UISpecs[ID].Dom).trigger("click");
            }
        )//clicking of the named element triggers a click on the
        */

        function call_renderSubEventsClickEvents(e)
        {
            e.stopPropagation();
            if (e.ctrlKey)
            {
                global_multiSelect.AddElement(ID);
                return;
            }
            renderSubEventsClickEvents(ID)
        }
        DayOfWeek.UISpecs[ID].refrenceListElement.Dom.onclick = call_renderSubEventsClickEvents;
    }

}





function renderSubEventsClickEvents(SubEventID)
{
    global_DictionaryOfSubEvents[SubEventID].Bind();
    global_DictionaryOfSubEvents[SubEventID].showBottomPanel();

    var DayContainer=global_DictionaryOfSubEvents[SubEventID].Day
    var refSubEvent = global_DictionaryOfSubEvents[SubEventID].ListRefElement
    setTimeout(function () { PositionIconSet(DayContainer, refSubEvent) },150);
}
renderSubEventsClickEvents.BottomPanelIsOpen = false;
renderSubEventsClickEvents.isRefListSubEventClicked = false;



        function renderUIChanges(DayOfWeek) {
    for (var ID in DayOfWeek.UISpecs) {
        DayOfWeek.UISpecs[ID].Dom.Active = false;


        /*
        if ((DayOfWeek.UISpecs[ID].Dom.Active))
        {
            DayOfWeek.UISpecs[ID].Dom.Active = false;
            DayOfWeek.UISpecs[ID].Enabled = false;
        }
        else
        {
            var OldDomID = ID + "_" + DayOfWeek.UISpecs[ID].OldIDindex;
            var toBeDeletedDom = getDomOrCreateNew(OldDomID);
    
            toBeDeletedDom.Dom.parentElement.removeChild(toBeDeletedDom.Dom);
            var retValue = delete DayOfWeek.UISpecs[SubEvent.ID];
        }
    
        */
        }
    }



        function genFunctionForSelectCalendarRange(ArrayOfCalendars, RefDate) {
            /*AllRanges[Index].Start = RangeOfWeek.Start;
            AllRanges[Index].End = RangeOfWeek.End;*/

    return function () {
        ArrayOfCalendars.forEach(function (eachRange) {
            if ((RefDate.getTime() >= eachRange.Start) && (RefDate.getTime() < eachRange.End)) {
                $(eachRange.Dom).show();
            }
            else {
                $(eachRange.Dom).hide();
        }

            //$(eachRange.Dom).show();
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
    var EndOfRange = new Date(StartOfRange.getTime() + (OneWeekInMs * global_RangeMultiplier));//sets the range to be used for query
    global_CurrentRange = { Start: StartOfRange, End: EndOfRange
        };

    var ScheduleRange = { Start: StartOfRange, End: EndOfRange
        };
            //getRangeofSchedule();
    var CurrentWeek = ScheduleRange.Start;
    var AllRanges = new Array();
    var AllWeekData = getDomOrCreateNew("CurrentWeekContainer");
    var index = 0;
    while (CurrentWeek < ScheduleRange.End)
    {
        //debugger;
        //CurrentWeek = CurrentWeek.dst() ? new Date(Number(CurrentWeek.getTime()) +OneHourInMs) : CurrentWeek;
        var MyRange = { Start: CurrentWeek, End: new Date(Number(CurrentWeek) + Number(OneWeekInMs))};

        var CurrentWeekDOm = genDivForEachWeek(MyRange, AllRanges);
        if (!CurrentWeekDOm.status) {
                AllWeekData.Dom.appendChild(CurrentWeekDOm.Dom);
                var translatePercent = 100 * index;
                CurrentWeekDOm.Dom.style.transform = 'translateX(' + translatePercent + '%)';
                $(CurrentWeekDOm.Dom).addClass("weekContainer");
                //CurrentWeekDOm.Dom.style.width = "100%";
                //CurrentWeekDOm.Dom.style.width = "100%";

                //var ctx = CurrentWeekDOm.Dom.getContext("2d");
                CurrentWeekDOm.index = index;
                CurrentWeek = new Date(Number(CurrentWeek) + Number(OneWeekInMs));
        }
        ++index;
    }

    ParentDom.appendChild(AllWeekData.Dom);
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
    AllDayDivs.forEach(function (obj) {
        AllDayContainer.Dom.appendChild(obj.Dom)
        var CallBackFunc = function () {
            //debugger;
            scrollToDay(obj.StartDate);
            dayTicker.Dom.style.left = obj.left + "%";
    }
        $(obj.Dom).click(CallBackFunc);
        if ((new Date() >= obj.StartDate) && (new Date() < obj.EndDate)) {
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
    var AllDayDiivs = new Array();
    var i = 0;
    while (IniMonth == Month)
    {
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

    for (i in AllRangeData) {
        var rangeElement = AllRangeData[i]
        var StartDate = new Date(Number(rangeElement.Start));
        var EndDate = new Date(Number(rangeElement.End));

        if (((SubEvent.ID === "11792_7_11978_11979"))) {
            var a = 9;
    }


        if ((SubEventStart >= StartDate) && (SubEventStart <= EndDate)) {
            ValidRange.push(rangeElement);
            rangeElement.SubEventsCollection[SubEvent.ID] = (SubEvent);

            if (rangeElement.UISpecs[SubEvent.ID] == null) {
                rangeElement.UISpecs[SubEvent.ID] = { css: { }, IDindex: SubEvent.CurrentIndex, OldIDindex: null, Enabled: true, Dom: null
            }
        }
            rangeElement.UISpecs[SubEvent.ID].Start = SubEvent.SubCalStartDate.getTime();
            rangeElement.UISpecs[SubEvent.ID].IDindex = SubEvent.CurrentIndex;
            rangeElement.UISpecs[SubEvent.ID].Enabled = true;
            rangeElement.UISpecs[SubEvent.ID].Dom = null;

    }
        if ((SubEventEnd >= StartDate) && (SubEventEnd <= EndDate)) {
            if (ValidRange.indexOf(rangeElement) < 0) {
                ValidRange.push(rangeElement);

                if (rangeElement.UISpecs[SubEvent.ID] == null) {
                    rangeElement.UISpecs[SubEvent.ID] = { css: { }, IDindex: SubEvent.CurrentIndex, OldIDindex: null, Enabled: true, Dom: null
                }
            }

                rangeElement.SubEventsCollection[SubEvent.ID] = SubEvent;
                rangeElement.UISpecs[SubEvent.ID].IDindex = SubEvent.CurrentIndex;
                rangeElement.UISpecs[SubEvent.ID].Enabled = true;
                rangeElement.UISpecs[SubEvent.ID].Dom = null;
                rangeElement.UISpecs[SubEvent.ID].Start = SubEvent.SubCalStartDate.getTime();
        }
    }
        }


    if (ValidRange.length > 0) {
        return ValidRange;
        }

    return ValidRange;
    }




        function getMyDom(SubEvent, Range) {
        var referenceStart = new Date(Range.Start);
            //referenceStart.setHours(refStart.Hour, refStart.Minute);
        var referenceEnd = new Date(Range.End);
            //referenceEnd.setHours(refEnd.Hour, refEnd.Minute);
        var SubCalCalEventStart = new Date(SubEvent.SubCalStartDate);
        var SubCalCalEventEnd = new Date(SubEvent.SubCalEndDate);
        if ((SubEvent.gridDoms == undefined) || (SubEvent.gridDoms == null)) {
            SubEvent.gridDoms = new Array();
        }

        if (SubCalCalEventStart > referenceStart) {
            referenceStart = SubCalCalEventStart;
        }

        if (referenceEnd > SubCalCalEventEnd) {
            referenceEnd = SubCalCalEventEnd;
        }

        if (SubEvent.ID == "110844_7_110850_110851") {
            //debugger;
            var z = 981;
        }

        var totalDuration = referenceEnd -referenceStart;
        var percentHeight = (totalDuration / OneDayInMs) * 100;

        var percentTop = ((referenceStart - new Date(Range.Start)) / OneDayInMs) * 100;
            function call_renderSubEventsClickEvents(e) {
            e.stopPropagation();
            renderSubEventsClickEvents(SubEvent.ID)
        }
        if (Range.UISpecs[SubEvent.ID].Enabled) {
            Range.UISpecs[SubEvent.ID].css = { height: 0, top: 0
        };
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
            

            if (SubEvent.ID == "110972_7_110976_110977")
            {
                //debugger;
            }
            $(EventDom.Dom).hover(function () {
               //setTimeout(function () { debugger; },100)
                
            });

            EventDom.setAttribute("Title", SubEvent.SubCalStartDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + SubEvent.SubCalEndDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
            //debugger;
            if (SubEvent.ColorSelection > 0) {
                //debugger;
                $(EventDom.Dom).addClass(global_AllColorClasses[SubEvent.ColorSelection].cssClass);
        }
            $(EventDom.Dom).addClass("SameSubEvent" + SubEvent.ID);

            //$(EventDom.Dom).click(prepOnClickOfCalendarElement(SubEvent, EventDom.Dom));
            global_DictionaryOfSubEvents[SubEvent.ID].showBottomPanel = prepOnClickOfCalendarElement(SubEvent, EventDom.Dom);
            global_DictionaryOfSubEvents[SubEvent.ID].Day = Range;

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
        else {
            if (!Range.UISpecs[SubEvent.ID].Enabled)//checks if it is active
            {
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
                //global_previousSelectedSubCalEvent.pop();
            }
            global_previousSelectedSubCalEvent = new Array();
        }
        function prepOnClickOfCalendarElement(SubEvent, Dom) {
        return function () {
            //event.stopPropagation();
            if (!renderSubEventsClickEvents.isRefListSubEventClicked)
            {
                global_ExitManager.triggerLastExitAndPop();
            }
            DeselectAllSideBarElements();
            
            
            var AllDomsOfTheSameSubevent = $(".SameSubEvent" + SubEvent.ID);
            for (var i = 0; i < AllDomsOfTheSameSubevent.length; i++) {
                var myDom = AllDomsOfTheSameSubevent[i]
                $(myDom).addClass("SelectedWeekGridSubcalEvent");
                global_previousSelectedSubCalEvent.push(myDom);
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
            var deleteButton = global_ControlPanelIconSet.getDeleteButton();
            var DeleteMessage = getDomOrCreateNew("DeleteMessage")
            var ProcatinationButton = getDomOrCreateNew("submitProcatination");
            var ProcatinationCancelButton = getDomOrCreateNew("cancelProcatination");
            var ControlPanelCloseButton = global_ControlPanelIconSet.getCloseButton();
            var ProcrastinateEventModalContainer = getDomOrCreateNew("ProcrastinateEventModal");
            var ControlPanelProcrastinateButton = global_ControlPanelIconSet.getProcrastinateButton();
            var ControlPanelLocationButton = global_ControlPanelIconSet.getLocationButton();
            var ModalDelete = getDomOrCreateNew("ConfirmDeleteModal")
            var ControlPanelContainer = getDomOrCreateNew("ControlPanelContainer");
            var MultiSelectPanel = getDomOrCreateNew("MultiSelectPanel");
            $(ControlPanelContainer).addClass("ControlPanelContainerModal");
            /*if (renderSubEventsClickEvents.BottomPanelIsOpen) {
                closeControlPanel();
            }*/

            var InfoPanelOverLay = getDomOrCreateNew("InfoPanelOverLay")
            removeLowerBarIconSetCOntainer();
            revealControlInfoContainer()
            var BasePanel = generateBasePanel();
            

            var IconSetContainer = global_ControlPanelIconSet.getIconSetContainer();
            $(ControlPanelContainer).addClass("setAsDisplayNone");
            setTimeout(function ()
            {
                $(ControlPanelContainer).removeClass("setAsDisplayNone");
                $(ControlPanelContainer).slideDown(500);
            }, 200)

            
            BasePanel.appendChild(InfoPanelOverLay);
            BasePanel.appendChild(IconSetContainer);
            

            ControlPanelContainer.appendChild(BasePanel);
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

            ControlPanelLocationButton.onclick = LauchLocation;

            ProcatinationButton.onclick = function () {
                debugger;
                procrastinateEvent();
                closeProcrastinatePanel();
            }


            ProcatinationCancelButton.onclick = closeProcrastinatePanel;

            function resetButtons() {
                yeaButton.onclick = null;
                nayButton.onclick = null;
                ControlPanelProcrastinateButton.onclick = null;
                ControlPanelCloseButton.onclick = null;
                ControlPanelLocationButton.onclick = null;
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

            function generateEditDoneCOntainer()
            {
                var SaveButton = getDomOrCreateNew("SaveButton", "button");
                SaveButton.innerHTML = "Save"
                var EditContainer = getDomOrCreateNew("EditCalEventContainer");
                $(EditContainer).addClass("setAsDisplayNone");
                EditContainer.appendChild(SaveButton);
                var EditContainerStatus = { isRevealed: false };
                function RevealEditContainer()
                {
                    if (!EditContainerStatus.isRevealed)
                    {
                        $(EditContainer).removeClass("setAsDisplayNone");
                        EditContainerStatus.isRevealed = true;
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
                TriggerClose();
            }


            function TriggerClose() {
                getRefreshedData.enableDataRefresh();
                resetButtons();
                DeselectAllSideBarElements();
                closeModalDelete();
                closeProcrastinatePanel();
                deleteButton.onclick = null;
                completeButton.onclick = null;
                $(ControlPanelContainer).slideUp(500);
                document.removeEventListener("keydown", containerKeyPress);
                renderSubEventsClickEvents.isRefListSubEventClicked = false;
                renderSubEventsClickEvents.BottomPanelIsOpen = false;
                
                if (IconSetContainer.parentNode != null) {
                    IconSetContainer.parentNode.removeChild(IconSetContainer);
                }
                $(ControlPanelContainer).removeClass("ControlPanelContainerLowerBar");
                setTimeout(function(){ $(ControlPanelContainer).removeClass("ControlPanelContainerModal")},500);
            }


            //checks if bottompannel is open. If panel is open then just reset the subevent reflist element as opposed to 
            if (!renderSubEventsClickEvents.isRefListSubEventClicked) {
                global_ExitManager.addNewExit(CombinedCLoser);
            }
            
            
            
            
            

            function closeProcrastinatePanel() {
                $(ProcrastinateEventModalContainer).slideUp(500);
            }

            ControlPanelCloseButton.onclick = global_ExitManager.triggerLastExitAndPop;



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
                    var DeletionEvent = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: SubEvent.ID, TimeZoneOffset: TimeZone
                };
                    //var URL = "RootWagTap/time.top?WagCommand=6"
                    var URL = global_refTIlerUrl + "Schedule/Event";
                    var HandleNEwPage = new LoadingScreenControl("Tiler is Deleting your event :)");
                    HandleNEwPage.Launch();

                    var exit = function (data) {
                        HandleNEwPage.Hide();
                        //triggerUIUPdate();//hack alert
                        global_ExitManager.triggerLastExitAndPop();
                        getRefreshedData();
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
                        getRefreshedData();
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

            function procrastinateEvent() {
                var HourInput = getDomOrCreateNew("procrastinateHours").value == "" ? 0 : getDomOrCreateNew("procrastinateHours").value;
                var MinInput = getDomOrCreateNew("procrastinateMins").value == "" ? 0 : getDomOrCreateNew("procrastinateMins").value;
                var DayInput = getDomOrCreateNew("procrastinateDays").value == "" ? 0 : getDomOrCreateNew("procrastinateDays").value;
                debugger;
                var TimeZone = new Date().getTimezoneOffset();
                var NowData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: SubEvent.ID, DurationDays: DayInput, DurationHours: HourInput, DurationMins: MinInput, TimeZoneOffset: TimeZone
            };
                //var URL= "RootWagTap/time.top?WagCommand=2";
                var URL = global_refTIlerUrl + "Schedule/Event/Procrastinate";
                var HandleNEwPage = new LoadingScreenControl("Tiler is Postponing  :)");
                HandleNEwPage.Launch();

                var exit = function (data) {
                    HandleNEwPage.Hide();
                    //triggerUIUPdate();//hack alert
                    global_ExitManager.triggerLastExitAndPop();
                    getRefreshedData();
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
                            //alert(response);
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
                    getRefreshedData();

                });

                function triggerUIUPdate() {
                    //resetButtons();
                    global_ExitManager.triggerLastExitAndPop();
                }
        }

            function markAsComplete() {
                SendMessage();
                function SendMessage() {
                    var TimeZone = new Date().getTimezoneOffset();
                    var Url;
                    //Url="RootWagTap/time.top?WagCommand=7";
                    Url = global_refTIlerUrl + "Schedule/Event/Complete";
                    var HandleNEwPage = new LoadingScreenControl("Tiler is updating your schedule ...");
                    HandleNEwPage.Launch();

                    var MarkAsCompleteData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: SubEvent.ID, TimeZoneOffset: TimeZone
                    };
                    var exit = function (data) {
                        HandleNEwPage.Hide();
                        //triggerUIUPdate();//hack alert
                        global_ExitManager.triggerLastExitAndPop();
                        getRefreshedData();
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
                        HandleNEwPage.Hide();
                        triggerUIUPdate();//hack alert
                        getRefreshedData();
                    });
            }
                function triggerUIUPdate() {
                    //resetButtons();
                    global_ExitManager.triggerLastExitAndPop();
            }

        }

            function closeModalDelete() {
                DeleteMessage.innerHTML = "Sure you want to delete ?"
                $('#ConfirmDeleteModal').slideUp(500);
                ModalDelete.isRevealed = false;
        }
            deleteButton.onclick = deleteSubevent;
            completeButton.onclick = markAsComplete;

            var ControlPanelContainer = getDomOrCreateNew("ControlPanelContainer");
            ControlPanelContainer.focus();

            function containerKeyPress(e) {
                //e.stopPropagation();
                
                if (e.which == 27)//escape key press
                {

                    return;//closeControlPanel();
                }

                if ((e.which == 8) || (e.which == 46))//bkspc/delete key pressed
                {
                    deleteSubevent();
                }
        }
            //debugger;
            document.removeEventListener("keydown", containerKeyPress);//this is here just to avooid duplicate addition of the same keypress event
            document.addEventListener("keydown", containerKeyPress);

            renderSubEventsClickEvents.isRefListSubEventClicked = true;
            renderSubEventsClickEvents.BottomPanelIsOpen = true;

            /*
            ControlPanelNameOfSubeventInfo.innerHTML = SubEvent.Name;
            ControlPanelDeadlineOfSubeventInfo.innerHTML = Deadline.hour + ' ' + Deadline.minute + ' ' + Deadline.merid + ' // ' + Deadline.day + ', ' + Deadline.mon + ' ' + Deadline.date;
            ControlPanelSubEventTimeInfo.innerHTML = StartDate.hour + ' ' + StartDate.minute + ' ' + StartDate.merid + ' &mdash; ' + EndDate.hour + ' ' + EndDate.minute + ' ' + EndDate.merid;
            */
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


            //ControlPanelNameOfSubeventInfo.value = SubEvent.Name;
            ControlPanelNameOfSubeventInfo.appendChild(NameContanierInput);


            function removeInputBindings()
            {
                
            }
            
            function extraOptionsData()
            {
                var splitInputBox = getDomOrCreateNew("InputSplitCount", "input");

                splitInputBox.oninput= EditContainerData.RevealContainer;
                splitInputBox.setAttribute("type", "Number");
                splitInputBox.onkeydown = stopPropagationOfKeyDown;
                splitInputBox.value = Dictionary_OfCalendarData[SubEvent.CalendarID].TotalNumberOfEvents;
                var splitInputBoxLabel = getDomOrCreateNew("splitInputBoxLabel","label");
                splitInputBoxLabel.innerHTML="Splits"
                var splitInputBoxContainer = getDomOrCreateNew("InputSplitCountContainer");
                splitInputBoxContainer.appendChild(splitInputBoxLabel);
                splitInputBoxContainer.appendChild(splitInputBox);
                $(splitInputBoxContainer).addClass("SubEventInformationContainer");

                var CompletionMap = getDomOrCreateNew("CompletionContainer");
                var CompletionMapDom= generateCompletionMap(SubEvent)
                CompletionMap .appendChild(CompletionMapDom);
                var ContainerForExtraOptions = getDomOrCreateNew("ExtraOptionsContainer")
                ContainerForExtraOptions.appendChild(splitInputBoxContainer)
                ContainerForExtraOptions.appendChild(CompletionMap)
                
                return ContainerForExtraOptions;
            }

            
            SubCalStartInfo.appendChild(SubEventStartTime);
            //SubCalTimeInfo.appendChild(AmDash0);
            //SubCalTimeInfo.appendChild(SubEventEndTime);
            SubCalStartInfo.appendChild(SubEventStartDate);

            SubCalEndInfo.appendChild(SubEventEndTime);
            //SubCalDateInfo.appendChild(AmDash1);
            SubCalEndInfo.appendChild(SubEventEndDate);


            ControlPanelSubEventTimeInfo.appendChild(SubCalStartInfo);
            ControlPanelSubEventTimeInfo.appendChild(SubCalEndInfo);
            

            var ControlPanelDeadlineOfSubevent=getDomOrCreateNew("ControlPanelDeadlineOfSubevent")
            //ControlPanelDeadlineOfSubeventInfo.appendChild(CalStartDate);
            ControlPanelDeadlineOfSubeventInfo.appendChild(CalEndTime)
            ControlPanelDeadlineOfSubeventInfo.appendChild(CalEndDate);
            var optionData = extraOptionsData();

            var InfoCOntainer = getDomOrCreateNew("InfoContainer")

            InfoCOntainer.appendChild(optionData);
            if (Dictionary_OfCalendarData[SubEvent.CalendarID].Rigid)
            {
                $(ControlPanelDeadlineOfSubevent).addClass("setAsDisplayNone");
                $(optionData).addClass("setAsDisplayNone");
                
            }
            else
            {
                $(ControlPanelDeadlineOfSubevent).removeClass("setAsDisplayNone");
                $(optionData).removeClass("setAsDisplayNone");
            }
            



            function SaveButtonClick()
            {
                debugger;
                var TimeZone = new Date().getTimezoneOffset();
                var Url;
                //Url="RootWagTap/time.top?WagCommand=7";
                SubEventStartTime.value =formatTimePortionOfStringToRightFormat(SubEventStartTime.value )
                var SubCalStartDateTimeString = SubEventStartTime.value + " " + $(SubEventStartDate).datepicker("getDate").toLocaleDateString();
                var SubCaStartDateInMS = Date.parse(SubCalStartDateTimeString) - (TimeZone * OneMinInMs)


                SubEventEndTime.value = formatTimePortionOfStringToRightFormat(SubEventEndTime.value)
                var SubCalEndDateTimeString = SubEventEndTime.value + " " + $(SubEventEndDate).datepicker("getDate").toLocaleDateString();
                var SubCaEndDateInMS = Date.parse(SubCalEndDateTimeString) - (TimeZone*OneMinInMs);//.getFullTime();

                CalEndTime.value = formatTimePortionOfStringToRightFormat(CalEndTime.value)

                var CalDateEndTimeString = CalEndTime.value + " " + $(CalEndDate).datepicker("getDate").toLocaleDateString();
                var CalEndDateInMS = Date.parse(CalDateEndTimeString) - (TimeZone * OneMinInMs);
                var splitValue =  getDomOrCreateNew("InputSplitCount", "input").value;
                Url = global_refTIlerUrl + "SubCalendarEvent/Update";
                var HandleNEwPage = new LoadingScreenControl("Tiler is updating your schedule ...");
                HandleNEwPage.Launch();

                var SaveData = {
                    UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: SubEvent.ID, EventName: NameContanierInput.value, TimeZoneOffset: TimeZone, Start: SubCaStartDateInMS, End: SubCaEndDateInMS, CalStart: 0, CalEnd: CalEndDateInMS, Split: splitValue
                };
                var exit= function (data) {
                    HandleNEwPage.Hide();
                    //triggerUIUPdate();//hack alert
                    global_ExitManager.triggerLastExitAndPop();
                    getRefreshedData();
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
    var WeekRenderPlaneID = WeekID +"RenderPlane"
    var RenderPlane = getDomOrCreateNew(WeekRenderPlaneID);
    $(RenderPlane.Dom).addClass("renderPlane");
    var prev;

    BindAddNewEventToClick(WeekRange);

    var StartOfDay = new Date(RangeOfWeek.Start);
    //StartOfDay = StartOfDay.dst() ? new Date(Number(StartOfDay.getTime())) : StartOfDay +OneHourInMs;
    if (!WeekRange.status) {
        var DaysOfWeekDoms = new Array();
        //WeekRange.Dom.appendChild(RenderPlane.Dom);
        WeekDays.forEach(
            function (DayOfWeek) {
                var myDay = generateDayContainer();
                //myDay.renderPlane = RenderPlane;
                myDay.widtPct = widthPercent;
                myDay.Start = new Date(StartOfDay);//set start of day property
                var TotalMilliseconds = myDay.Start.getTime();
                TotalMilliseconds += OneDayInMs
                StartOfDay = new Date(TotalMilliseconds);
                if (StartOfDay.getHours() != 0) {
                    TotalMilliseconds += OneHourInMs;
                    StartOfDay = new Date(TotalMilliseconds);
            }
                myDay.End = new Date(TotalMilliseconds -1);

                var currDate = new Date(Number(refDate.getTime()) + Number(DayIndex * OneDayInMs))
                if (new Date().dst()) {
                    currDate = new Date(Number(currDate.getTime()) +OneHourInMs);
                    // myDay.End = new Date(Number(myDay.End.getTime()) + OneHourInMs);
            }
                prev = currDate;
                var Month = currDate.getMonth() +1;
                var Day = currDate.getDate();
                myDay.NameOfDayContainer.Dom.innerHTML = "<p>" + DayOfWeek.substring(0, 2) + "<p class='dateOfDay'>" +myDay.Start.getDate()
                ;;
                //   + "<br/><span>" + Month + "/" + Day + "</span></p>";;


                myDay.SubEventsCollection = {
            };
                BindClickTOStartOfDay(myDay);
                myDay.UISpecs = {
                };

                myDay.WeekRenderPlane = RenderPlane;
                $(myDay.Parent.Dom).addClass(DayOfWeek + "DayContainer");
                RenderPlane.appendChild(myDay.Parent.Dom);
                //WeekRange.Dom.appendChild(myDay.Parent.Dom);
                myDay.LeftPercent = (DayIndex * widthPercent);
                myDay.RightPercent = myDay.LeftPercent + widthPercent;

                DayIndex += 1;
                DaysOfWeekDoms.push(myDay);
        });

        WeekRange.DaysOfWeek = DaysOfWeekDoms;
        var Index =AllRanges.length//gets index, because it gets the index bewfore the push
        AllRanges.push(WeekRange);
        AllRanges[Index].Start = RangeOfWeek.Start;
        AllRanges[Index].End = RangeOfWeek.End;
        AllRanges[Index].UISpecs = {
    };
        AllRanges[Index].SubEventsCollection = {
    };
        }
    WeekRange.renderPlane = RenderPlane;

            //binds click event to creation of new event


    WeekRange.Dom.appendChild(RenderPlane.Dom);

    return WeekRange;
    }

        //function creates Bind a click event to the render plane to enable addition of new events
        function BindAddNewEventToClick(Week) {
    var RenderPlaneDom = Week.Dom;
    $(RenderPlaneDom).click(function (e) {
        var posX = $(this).offset().left
        var posY = $(this).offset().top;
        var left = e.pageX -posX;
        var top = e.pageY -posY;
        var height = $(RenderPlaneDom).height();
        var width = $(RenderPlaneDom).width();
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



