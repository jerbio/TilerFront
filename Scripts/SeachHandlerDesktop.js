function ActivateUserSearch(e)
{
    var SearchInput = getDomOrCreateNew("SearchBar");
    var SearchContainer = getDomOrCreateNew("SearchBarAndContentContainer");
    if (e.shiftKey || e.ctrlKey || e.altKey || (!ActivateUserSearch.isSearchOn))
    {
        return;
    }
    if (e.which == 27)
    {
        if (ActivateUserSearch.isActive)
        {
            $(SearchContainer.Dom).removeClass("FullScreenSearchContainer");
            SearchInput.Dom.value = "";
            ActivateUserSearch.AutoSuggest.clear();
            SearchInput.Dom.blur();
            ActivateUserSearch.isActive = false;
            return;
        }
        else
        {
            return;
        }
    }


    if ((ActivateUserSearch.isActive))
    {
        return;
    }

    if((e.which<46)||(e.which>90))
    {
        return;
    }

    var url = global_refTIlerUrl + "CalendarEvent/Name";
    var AutoSuggestSearch = new AutoSuggestControl(url, "GET", CallBackFunctionForReturnedValuesDesktop, SearchInput.Dom);
    ActivateUserSearch.AutoSuggest = AutoSuggestSearch;
    SearchContainer.Dom.appendChild(AutoSuggestSearch.getAutoSuggestControlContainer());
    SearchInput.Dom.focus();
    $(SearchContainer.Dom).addClass("FullScreenSearchContainer");
    ActivateUserSearch.isActive = true;
    
}

ActivateUserSearch.setSearchAsOff = function ()
{
    document.onkeydown = null;
    ActivateUserSearch.isSearchOn = false;
}

ActivateUserSearch.setSearchAsOn = function () {
    document.onkeydown = ActivateUserSearch;
    ActivateUserSearch.isSearchOn = true;
}

ActivateUserSearch.getSearch = function ()
{
    ActivateUserSearch.isSearchOn = searchStatus;
}
ActivateUserSearch.isSearchOn = true;
ActivateUserSearch.isActive = false;
document.onkeydown = ActivateUserSearch;

function EventSearch(InputDon)
{
    var isSearchOn = true;
    var isSearchActive = false;
    var SearchInput = getDomOrCreateNew("SearchBar");
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



    function PopulateDoNowButtonDom(MyCalendarEVent) {
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
        
        DoNowButtonOfSearchedEventContainer.Dom.appendChild(DoNowButtonOfSearchedEventImage.Dom)
        DoNowButtonOfSearchedEventContainer.Dom.appendChild(MyIconSet.getIconSetContainer());

        $(DoNowButtonOfSearchedEventImage.Dom).click(genFunctionCallForCalendarEventNow(MyCalendarEVent.ID))


        $(DoNowButtonOfSearchedEventContainer.Dom).addClass("DoNowButtonOfSearchedEventContainer");
        return DoNowButtonOfSearchedEventContainer;
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
                if (InitializeHomePage != null) {
                    InitializeHomePage();
                }
            });
        }
    }



    CallBackFunctionForReturnedValuesDesktop.counter = 0;
    //alert(data.length);
}

CallBackFunctionForReturnedValuesDesktop.counter = 0;