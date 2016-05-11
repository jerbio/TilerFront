function ActivateUserSearch(e)
{
    e.stopPropagation();
    var SearchInput = getDomOrCreateNew("SearchBarInput");
    var SearchContainer = getDomOrCreateNew("SearchBarAndContentContainer");
    if (e.shiftKey || e.ctrlKey || e.altKey || (!ActivateUserSearch.isSearchOn))
    {
        return;
    }


    if (e.which == 27)
    {
        return;

        /*
        if (ActivateUserSearch.isActive)
        {
            ActivateUserSearch.ClearSearch();
            return;
        }
        else
        {
            return;
        }
        */
    }


    if ((ActivateUserSearch.isActive))
    {
        return;
    }
    //debugger;
    if(((e.which<48)||(e.which>90))&&(e.which!=1))
    {
        return;
    }
    global_ExitManager.triggerLastExitAndPop();
    var url = global_refTIlerUrl + "CalendarEvent/Name";
    var AutoSuggestSearch = new AutoSuggestControl(url, "GET", CallBackFunctionForReturnedValuesDesktop, SearchInput.Dom);
    ActivateUserSearch.AutoSuggest = AutoSuggestSearch;
    SearchContainer.Dom.appendChild(AutoSuggestSearch.getAutoSuggestControlContainer());
    SearchInput.Dom.focus();
    $(SearchContainer.Dom).addClass("FullScreenSearchContainer");
    //SearchContainer.onkeydown = ActivateUserSearch;
    //SearchInput.onkeydown = ActivateUserSearch;
    ActivateUserSearch.isActive = true;
    getRefreshedData.disableDataRefresh();
    //ActivateUserSearch.setSearchAsOff();
    var SearchBar = getDomOrCreateNew("SearchBar")
    $(SearchBar).addClass("ActiveSearchBar")
    ActivateUserSearch.ClearSearch = function () {
        $(SearchContainer.Dom).removeClass("FullScreenSearchContainer");
        SearchInput.value = "";
        ActivateUserSearch.AutoSuggest.clear();
        SearchInput.Dom.blur();
        ActivateUserSearch.isActive = false;
        $(SearchBar).removeClass("ActiveSearchBar")
        getRefreshedData.enableDataRefresh(true);
    }
    global_ExitManager.addNewExit(ActivateUserSearch.ClearSearch);
    
    AddCloseButoon(SearchContainer, false);
}



ActivateUserSearch.setSearchAsOff = function ()
{
    //document.onkeydown = null;
    ActivateUserSearch.isSearchOn = false;
}

ActivateUserSearch.setSearchAsOn = function () {
    //document.onkeydown = ActivateUserSearch;
    ActivateUserSearch.isSearchOn = true;
}

ActivateUserSearch.getSearch = function ()
{
    ActivateUserSearch.isSearchOn = searchStatus;
}
ActivateUserSearch.isSearchOn = true;
ActivateUserSearch.isActive = false;
$(document).on("keydown", ActivateUserSearch);
setTimeout(function () {
    document.getElementById("SearchBar").onclick = ActivateUserSearch;
}, 1000);





function EventSearch(InputDon)
{
    var isSearchOn = true;
    var isSearchActive = false;
    var SearchInput = getDomOrCreateNew("SearchBarInput");
    var SearchContainer = getDomOrCreateNew("SearchBarAndContentContainer");
    var url = global_refTIlerUrl + "CalendarEvent/Name";
    var AutoSuggestSearch = new AutoSuggestControl(url, "GET", CallBackFunctionForReturnedValuesDesktop, SearchInput.Dom);
    var BindSearh = function ()
    {
        return function ()
        {
            TurnOffSearch();
            SearchContainer.Dom.appendChild(AutoSuggestSearch.getAutoSuggestControlContainer());
            SearchInput.Dom.focus();
            $(SearchContainer.Dom).addClass("FullScreenSearchContainer");
            isSearchActive = true;
        }
    }

    var TurnOffSearch = function ()
    {
        document.onkeydown = null;
        isSearchOn = false;
    }

    var TurnOnSearch = function ()
    {
        isSearchOn = true;
    }
}

function CallBackFunctionForReturnedValuesDesktop(data, DomContainer) {
    var EventIndex = 0;
    var HeightOfDom = 50;

    $(DomContainer.Dom).empty();
    (DomContainer.Dom).style.height = 0;
    if (data.length == 0 || data.length == null || data.length == undefined) {
        DomContainer.innerHTML = "No Results";
        setTimeout(function () { $(DomContainer.Dom).empty(); }, 6000);
        return;
    }



    data.forEach(resolveEachRetrievedEvent);
    function resolveEachRetrievedEvent(CalendarEvent) {
        var CalendarEventDom = generateDOM(CalendarEvent);
        //CalendarEventDom.Dom.style.top = (EventIndex * HeightOfDom) + "px";
        //CalendarEventDom.Dom.style.height = HeightOfDom + "px"
        DomContainer.Dom.appendChild(CalendarEventDom.Dom);
        DomContainer.Dom.style.height = (++EventIndex * HeightOfDom) + "px";
    }



    function generateDOM(myCalendarEvent) {
        var retrievedEventContainerID = "retrievedEvent" + CallBackFunctionForReturnedValuesDesktop.counter
        var retrievedEventContainer = getDomOrCreateNew(retrievedEventContainerID);

        var TopPanelContainerContainerID = "TopPanelContainer" + CallBackFunctionForReturnedValuesDesktop.counter
        var TopPanelContainerContainer = getDomOrCreateNew(TopPanelContainerContainerID);
        $(TopPanelContainerContainer.Dom).addClass("TopPanelContainer");

        var NameOfSearchedEventContainer = PopulateNameDom(myCalendarEvent);
        var RepetitionOfSearchedEventContainer = PopulateRepetitionDom(myCalendarEvent);


        var CompletionGraphOfSearchedEventContainerID = "CompletionGraphOfSearchedEventContainer" + CallBackFunctionForReturnedValuesDesktop.counter
        var CompletionGraphOfSearchedEventContainer = getDomOrCreateNew(CompletionGraphOfSearchedEventContainerID);
        $(CompletionGraphOfSearchedEventContainer.Dom).addClass("CompletionGraphOfSearchedEventContainer");

        //var DoNowButtonOfSearchedEventContainerID = "DoNowButtonOfSearchedEventContainer" + CallBackFunctionForReturnedValuesDesktop.counter
        var DoNowButtonOfSearchedEventContainer = PopulateDoNowButtonDom(myCalendarEvent)
        //$(DoNowButtonOfSearchedEventContainer.Dom).addClass("DoNowButtonOfSearchedEventContainer");


        TopPanelContainerContainer.Dom.appendChild(NameOfSearchedEventContainer.Dom);
        //    NameOfSearchedEventContainer.Dom.style.width = "85%";//hack alert
        //TopPanelContainerContainer.Dom.appendChild(RepetitionOfSearchedEventContainer.Dom);
        //TopPanelContainerContainer.Dom.appendChild(CompletionGraphOfSearchedEventContainer.Dom);
        TopPanelContainerContainer.Dom.appendChild(DoNowButtonOfSearchedEventContainer.Dom);


        //var DeadlineOfSearchedEventContainerID = "DeadlineOfSearchedEventContainer" + CallBackFunctionForReturnedValuesDesktop.counter
        var DeadlineOfSearchedEventContainer = PopulateDeadlineDom(myCalendarEvent); //getDomOrCreateNew(DeadlineOfSearchedEventContainerID);

        retrievedEventContainer.Dom.appendChild(TopPanelContainerContainer.Dom);
        retrievedEventContainer.Dom.appendChild(DeadlineOfSearchedEventContainer.Dom);
        $(retrievedEventContainer.Dom).addClass("retrievedEvent");


        ++CallBackFunctionForReturnedValuesDesktop.counter;
        return retrievedEventContainer;
    }

    function PopulateNameDom(MyCalendarEVent) {
        var NameOfSearchedEventContainerID = "NameOfSearchedEventContainer" + CallBackFunctionForReturnedValuesDesktop.counter
        var NameOfSearchedEventContainer = getDomOrCreateNew(NameOfSearchedEventContainerID);
        var NameOfSearchedEventTextID = "NameOfSearchedEventText" + CallBackFunctionForReturnedValuesDesktop.counter
        NameOfSearchedEventContainer.Dom.innerHTML = MyCalendarEVent.CalendarName
        // NameOfSearchedEventContainer.Dom.style.fontSize = HeightOfDom / 4 + "px";
        // NameOfSearchedEventContainer.Dom.style.lineHeight = HeightOfDom + "px";

        $(NameOfSearchedEventContainer.Dom).addClass("NameOfSearchedEventContainer");
        return NameOfSearchedEventContainer;
    }



    function PopulateRepetitionDom(MyCalendarEVent) {
        var RepetitionOfSearchedEventContainerID = "RepetitionOfSearchedEventContainer" + CallBackFunctionForReturnedValuesDesktop.counter
        var RepetitionOfSearchedEventContainer = getDomOrCreateNew(RepetitionOfSearchedEventContainerID);
        var RepetitionOfSearchedEventTextID = "RepetitionOfSearchedEventText" + CallBackFunctionForReturnedValuesDesktop.counter
        var RepetitionOfSearchedEventText = getDomOrCreateNew(RepetitionOfSearchedEventContainerID, "span");
        RepetitionOfSearchedEventText.Dom.innerHTML = MyCalendarEVent.CalendarRepetition
        $(RepetitionOfSearchedEventContainer.Dom).addClass("RepetitionOfSearchedEventContainer");
        return RepetitionOfSearchedEventContainer;
    }


    function PopulateDeadlineDom(MyCalendarEVent) {
        var DeadlineOfSearchedEventContainerID = "DeadlineOfSearchedEventContainer" + CallBackFunctionForReturnedValuesDesktop.counter
        var DeadlineOfSearchedEventContainer = getDomOrCreateNew(DeadlineOfSearchedEventContainerID);
        var DeadlineOfSearchedEventTextID = "DeadlineOfSearchedEventText" + CallBackFunctionForReturnedValuesDesktop.counter
        var DeadlineDate = new Date(MyCalendarEVent.EndDate);
        DeadlineOfSearchedEventContainer.Dom.innerHTML = "Ends: " + DeadlineDate.toLocaleDateString();



        $(DeadlineOfSearchedEventContainer.Dom).addClass("DeadlineOfSearchedEventContainer");
        return DeadlineOfSearchedEventContainer;
    }



    function PopulateDoNowButtonDom(MyCalendarEVent)
    {
        var DoNowButtonOfSearchedEventContainerID = "DoNowButtonOfSearchedEventContainer" + CallBackFunctionForReturnedValuesDesktop.counter
        var DoNowButtonOfSearchedEventContainer = getDomOrCreateNew(DoNowButtonOfSearchedEventContainerID);
        var DoNowButtonOfSearchedEventImageID = "DoNowButtonOfSearchedEventImage" + CallBackFunctionForReturnedValuesDesktop.counter
        var DoNowButtonOfSearchedEventImage = getDomOrCreateNew(DoNowButtonOfSearchedEventImageID);


        $(DoNowButtonOfSearchedEventImage.Dom).addClass("NowIcon");
        $(DoNowButtonOfSearchedEventImage.Dom).addClass("NowIcon_Search");
        var MyIconSet = new IconSet();
        var IconSetContainer = MyIconSet.getIconSetContainer();
        $(IconSetContainer).addClass("IconSetSearch");
        $(MyIconSet.getCloseButton()).addClass("setAsDisplayNone");
        $(MyIconSet.getLocationButton()).addClass("setAsDisplayNone");
        $(MyIconSet.getProcrastinateButton()).addClass("setAsDisplayNone");
        $(MyIconSet.getPauseResumeButton()).addClass("setAsDisplayNone");
        
        var DeleteButton = MyIconSet.getDeleteButton();
        DeleteButton.onclick = function () { DeleteTrigger(DoNowButtonOfSearchedEventContainer, MyCalendarEVent, function () { }) }
        var CompletionButton = MyIconSet.getCompleteButton();
        CompletionButton.onclick = function () {
            prepCompletion(MyCalendarEVent)();
        }


        DoNowButtonOfSearchedEventContainer.Dom.appendChild(DoNowButtonOfSearchedEventImage.Dom)
        DoNowButtonOfSearchedEventContainer.Dom.appendChild(MyIconSet.getIconSetContainer());

        

        function DeleteTrigger(Container,Event, Exit)
        {
            var DeletionSelectionID = "DeleteCalEventContainer" + CallBackFunctionForReturnedValuesDesktop.counter
            var DeletionSelection = getDomOrCreateNew(DeletionSelectionID)
            $(DeletionSelection).addClass("DeleteSearchContainer");
            var DeletionMessageID = "DeletionMessage" + CallBackFunctionForReturnedValuesDesktop.counter;
            var DeletionMessage = getDomOrCreateNew(DeletionMessageID)
            DeletionMessage.innerHTML = "Sure you want to delete \"" + Event.CalendarName + "\"";
            
            var YayDeleteButtonID = "YayDeleteButton" + +CallBackFunctionForReturnedValuesDesktop.counter;
            var YayDeleteButton = getDomOrCreateNew(YayDeleteButtonID, "button");
            YayDeleteButton.innerHTML = "Yea"
            var NayDeleteButtonID = "NayDeleteButton" + +CallBackFunctionForReturnedValuesDesktop.counter;
            var NayDeleteButton = getDomOrCreateNew(NayDeleteButtonID, "button");
            NayDeleteButton.innerHTML="Nay"
            DeletionSelection.appendChild(DeletionMessage);
            DeletionSelection.appendChild(YayDeleteButton);
            DeletionSelection.appendChild(NayDeleteButton);
            YayDeleteButton.onclick = function ()
            {
                //debugger;
                prepDeletion(Event)();
                CleanUP();
            }

            NayDeleteButton.onclick = function () {
                CleanUP();
            }

            function CleanUP()
            {
                if (DeletionSelection.parentElement != null) {
                    DeletionSelection.parentElement.removeChild(DeletionSelection)
                }
                Exit();
            }

            Container.appendChild(DeletionSelection);
            
            
        }
        function ProcratinateEvent(Container,Event, Exit)
        {
            var ProcrastinationContainerID = "ProcrastinationContainer" + CallBackFunctionForReturnedValuesDesktop.counter
            var ProcrastinationContainer = getDomOrCreateNew(ProcrastinationContainerID);
            var HourInputBoxID = "ProcrastinateHourInputBox" + CallBackFunctionForReturnedValuesDesktop.counter;
            var HourInputBox = getDomOrCreateNew(HourInputBoxID,"input");
            var MinInputBoxID = "ProcrastinateMinInputBox" + CallBackFunctionForReturnedValuesDesktop.counter;
            var MinInputBox = getDomOrCreateNew(MinInputBoxID, "input");
            var DayInputBoxID = "ProcrastinateDayInputBox" + CallBackFunctionForReturnedValuesDesktop.counter;
            var DayInputBox = getDomOrCreateNew(DayInputBoxID, "input");
            var SubmitID = "ProcrastinateSubmit" + CallBackFunctionForReturnedValuesDesktop.counter;
            var SubmitBox = getDomOrCreateNew(SubmitID, "input");
            ProcrastinationContainer.appendChild(HourInputBox);
            ProcrastinationContainer.appendChild(MinInputBox);
            ProcrastinationContainer.appendChild(DayInputBox);
            ProcrastinationContainer.appendChild(SubmitBox);
            Container.appendChild(ProcrastinationContainer);


            function CleanUP()
            {
                if (ProcrastinationContainer.parentElement != null) {
                    ProcrastinationContainer.parentElement.removeChild(ProcrastinationContainer)
                }
                Exit()
            }
        }
        $(DoNowButtonOfSearchedEventImage.Dom).click(genFunctionCallForCalendarEventNow(MyCalendarEVent.ID))
        $(DoNowButtonOfSearchedEventContainer.Dom).addClass("DoNowButtonOfSearchedEventContainer");
        return DoNowButtonOfSearchedEventContainer;
    }


    function prepDeletion(Event)
    {
        return function ()
        {
            /*
            alert("before marking as deleted");
            return;
            */
            var HandleNEwPage = new LoadingScreenControl("Tiler is Deleting The Event " + Event.CalendarName);
            HandleNEwPage.Launch();
            var deletionFailure = function ()
            {
                var NewMessage = "Oh No!!! Tiler is having issues modifying your schedule. Please try again Later :(";
                var ExitAfter = { ExitNow: true, Delay: 1000 };
                HandleNEwPage.UpdateMessage(NewMessage, ExitAfter);
                //debugger;
                ActivateUserSearch.ClearSearch();
            }

            var deletionSuccess = function () {
                HandleNEwPage.Hide();
                //debugger;
                ActivateUserSearch.ClearSearch();
            }

            var doneDeletion = function()
            {
                HandleNEwPage.Hide();
                //debugger;
                ActivateUserSearch.ClearSearch();
            }

            deleteCalendarEvent(Event.ID, deletionSuccess, deletionFailure, doneDeletion);
        }
    }

    function prepCompletion(Event) {
        return function () {
            /*
            alert("before marking as complete");
            return;
            */
            var HandleNEwPage = new LoadingScreenControl("Tiler is Marking \"" + Event.CalendarName + "\" as complete :)");
            HandleNEwPage.Launch();
            var completionFailure = function () {
                var NewMessage = "Oh No!!! Tiler is having issues modifying your schedule. Please try again Later :(";
                var ExitAfter = { ExitNow: true, Delay: 1000 };
                HandleNEwPage.UpdateMessage(NewMessage, ExitAfter);
                //debugger;
                ActivateUserSearch.ClearSearch();
            }

            var completionSuccess = function () {
                HandleNEwPage.Hide();
                //debugger;
                ActivateUserSearch.ClearSearch();
            }

            var doneCompletion = function () {
                HandleNEwPage.Hide();
                //debugger;
                ActivateUserSearch.ClearSearch();
            }

            completeCalendarEvent(Event.ID, completionSuccess, completionFailure, doneCompletion);
        }
    }

    

    function genFunctionCallForCalendarEventNow(CalendarEventID) {
        return function () {
            var TimeZone = new Date().getTimezoneOffset();

            //var Url ="RootWagTap/time.top?WagCommand=8";

            var HandleNEwPage = new LoadingScreenControl("Tiler is Moving Up Your Event...:)");
            HandleNEwPage.Launch();

            var Url = global_refTIlerUrl + "CalendarEvent/Now/";
            var NowData = { UserName: UserCredentials.UserName, ID: CalendarEventID, UserID: UserCredentials.ID, TimeZoneOffset: TimeZone };
            $.ajax({
                type: "POST",
                url: Url,
                data: NowData,
                success: function () {
                    HandleNEwPage.Hide();
                    ActivateUserSearch.ClearSearch();
                },
                error: function (err) {
                    var NewMessage = "Oh No!!! Tiler is having issues modifying your schedule. Please try again Later :(";
                    var ExitAfter = { ExitNow: true, Delay: 1000 };
                    HandleNEwPage.UpdateMessage(NewMessage, ExitAfter, InitializeHomePage);
                }
                // DO NOT SET CONTENT TYPE to json
                // contentType: "application/json; charset=utf-8", 
                // DataType needs to stay, otherwise the response object
                // will be treated as a single string
            }).done(function (data) {
                ActivateUserSearch.ClearSearch();
            });
        }
    }



    CallBackFunctionForReturnedValuesDesktop.counter = 0;
    //alert(data.length);
}

CallBackFunctionForReturnedValuesDesktop.counter = 0;