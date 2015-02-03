"use strict"
var NumberOfDom = 0;
function getSearchOverlay()
{
    var SearchPanelOverlayID = "SearchPanelOverlay";
    var SearchPanelOverlay = getDomOrCreateNew(SearchPanelOverlayID);
    SearchPanelOverlay.Dom.style.width = "100%";
    SearchPanelOverlay.Dom.style.left = "100%";
    $(SearchPanelOverlay.Dom).show()
}

function CallBackFunctionForReturnedValues(data, DomContainer) {
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
        CalendarEventDom.Dom.style.top = (EventIndex * HeightOfDom) + "px";
        CalendarEventDom.Dom.style.height = HeightOfDom + "px"
        DomContainer.Dom.appendChild(CalendarEventDom.Dom);
        DomContainer.Dom.style.height = (++EventIndex * HeightOfDom) + "px";
    }



    function generateDOM(myCalendarEvent) {
        var retrievedEventContainerID = "retrievedEvent" + CallBackFunctionForReturnedValues.counter
        var retrievedEventContainer = getDomOrCreateNew(retrievedEventContainerID);

        var TopPanelContainerContainerID = "TopPanelContainer" + CallBackFunctionForReturnedValues.counter
        var TopPanelContainerContainer = getDomOrCreateNew(TopPanelContainerContainerID);
        $(TopPanelContainerContainer.Dom).addClass("TopPanelContainer");

        var NameOfSearchedEventContainer = PopulateNameDom(myCalendarEvent);
        var RepetitionOfSearchedEventContainer = PopulateRepetitionDom(myCalendarEvent);


        var CompletionGraphOfSearchedEventContainerID = "CompletionGraphOfSearchedEventContainer" + CallBackFunctionForReturnedValues.counter
        var CompletionGraphOfSearchedEventContainer = getDomOrCreateNew(CompletionGraphOfSearchedEventContainerID);
        $(CompletionGraphOfSearchedEventContainer.Dom).addClass("CompletionGraphOfSearchedEventContainer");

        //var DoNowButtonOfSearchedEventContainerID = "DoNowButtonOfSearchedEventContainer" + CallBackFunctionForReturnedValues.counter
        var DoNowButtonOfSearchedEventContainer = PopulateDoNowButtonDom(myCalendarEvent)
        //$(DoNowButtonOfSearchedEventContainer.Dom).addClass("DoNowButtonOfSearchedEventContainer");


        TopPanelContainerContainer.Dom.appendChild(NameOfSearchedEventContainer.Dom);
        NameOfSearchedEventContainer.Dom.style.width = "85%";//hack alert
        //TopPanelContainerContainer.Dom.appendChild(RepetitionOfSearchedEventContainer.Dom);
        //TopPanelContainerContainer.Dom.appendChild(CompletionGraphOfSearchedEventContainer.Dom);
        TopPanelContainerContainer.Dom.appendChild(DoNowButtonOfSearchedEventContainer.Dom);


        //var DeadlineOfSearchedEventContainerID = "DeadlineOfSearchedEventContainer" + CallBackFunctionForReturnedValues.counter
        var DeadlineOfSearchedEventContainer = PopulateDeadlineDom(myCalendarEvent); //getDomOrCreateNew(DeadlineOfSearchedEventContainerID);

        retrievedEventContainer.Dom.appendChild(TopPanelContainerContainer.Dom);
        retrievedEventContainer.Dom.appendChild(DeadlineOfSearchedEventContainer.Dom);
        $(retrievedEventContainer.Dom).addClass("retrievedEvent");


        ++CallBackFunctionForReturnedValues.counter;
        return retrievedEventContainer;
    }

    function PopulateNameDom(MyCalendarEVent) {
        var NameOfSearchedEventContainerID = "NameOfSearchedEventContainer" + CallBackFunctionForReturnedValues.counter
        var NameOfSearchedEventContainer = getDomOrCreateNew(NameOfSearchedEventContainerID);
        var NameOfSearchedEventTextID = "NameOfSearchedEventText" + CallBackFunctionForReturnedValues.counter
        NameOfSearchedEventContainer.Dom.innerHTML = MyCalendarEVent.CalendarName
        NameOfSearchedEventContainer.Dom.style.fontSize = HeightOfDom / 4 + "px";
        NameOfSearchedEventContainer.Dom.style.lineHeight = HeightOfDom + "px";

        $(NameOfSearchedEventContainer.Dom).addClass("NameOfSearchedEventContainer");
        return NameOfSearchedEventContainer;
    }



    function PopulateRepetitionDom(MyCalendarEVent) {
        var RepetitionOfSearchedEventContainerID = "RepetitionOfSearchedEventContainer" + CallBackFunctionForReturnedValues.counter
        var RepetitionOfSearchedEventContainer = getDomOrCreateNew(RepetitionOfSearchedEventContainerID);
        var RepetitionOfSearchedEventTextID = "RepetitionOfSearchedEventText" + CallBackFunctionForReturnedValues.counter
        var RepetitionOfSearchedEventText = getDomOrCreateNew(RepetitionOfSearchedEventContainerID, "span");
        RepetitionOfSearchedEventText.Dom.innerHTML = MyCalendarEVent.CalendarRepetition
        $(RepetitionOfSearchedEventContainer.Dom).addClass("RepetitionOfSearchedEventContainer");
        return RepetitionOfSearchedEventContainer;
    }


    function PopulateDeadlineDom(MyCalendarEVent) {
        var DeadlineOfSearchedEventContainerID = "DeadlineOfSearchedEventContainer" + CallBackFunctionForReturnedValues.counter
        var DeadlineOfSearchedEventContainer = getDomOrCreateNew(DeadlineOfSearchedEventContainerID);
        var DeadlineOfSearchedEventTextID = "DeadlineOfSearchedEventText" + CallBackFunctionForReturnedValues.counter
        var DeadlineDate = new Date(MyCalendarEVent.EndDate);
        DeadlineOfSearchedEventContainer.Dom.innerHTML = "Ends: " + DeadlineDate.toLocaleDateString();



        $(DeadlineOfSearchedEventContainer.Dom).addClass("DeadlineOfSearchedEventContainer");
        return DeadlineOfSearchedEventContainer;
    }



    function PopulateDoNowButtonDom(MyCalendarEVent) {
        var DoNowButtonOfSearchedEventContainerID = "DoNowButtonOfSearchedEventContainer" + CallBackFunctionForReturnedValues.counter
        var DoNowButtonOfSearchedEventContainer = getDomOrCreateNew(DoNowButtonOfSearchedEventContainerID);
        var DoNowButtonOfSearchedEventImageID = "DoNowButtonOfSearchedEventImage" + CallBackFunctionForReturnedValues.counter
        var DoNowButtonOfSearchedEventImage = getDomOrCreateNew(DoNowButtonOfSearchedEventImageID);


        $(DoNowButtonOfSearchedEventImage.Dom).addClass("NowIcon");
        $(DoNowButtonOfSearchedEventImage.Dom).addClass("NowIcon_Search");
        //$(DoNowButtonOfSearchedEventImage.Dom).addClass("SearchIcon");


        DoNowButtonOfSearchedEventContainer.Dom.appendChild(DoNowButtonOfSearchedEventImage.Dom)

        $(DoNowButtonOfSearchedEventContainer.Dom).click(genFunctionCallForCalendarEventNow(MyCalendarEVent.ID))


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
                if (InitializeHomePage!=null)
                {
                    InitializeHomePage();
                }
            });
        }
    }



    CallBackFunctionForReturnedValues.counter = 0;
    //alert(data.length);
}

function generateSearchBarContainer(ParentDom)
{
    var url = global_refTIlerUrl + "CalendarEvent/Name";// "RootWagTap/time.top?WagCommand=10";
    var SearchAutoSuggest = new AutoSuggestControl(url, "GET", CallBackFunctionForReturnedValues);
    var DisablePanel = document.getElementById("DisablePanel");
    var TopBannerDisablePanel = document.getElementById("TopBannerDisablePanel");

    $(TopBannerDisablePanel).show();
    DisablePanel.style.zIndex = 10;
    TopBannerDisablePanel.style.zIndex = 3;
    $(DisablePanel).show();

    

    ParentDom = ParentDom.data[0];
    ParentDom.appendChild(SearchAutoSuggest.getAutoSuggestControlContainer());
    var SearchForEventFullContainer = SearchAutoSuggest.getAutoSuggestControlContainer();
    SearchForEventFullContainer.style.width = "70%";
    SearchForEventFullContainer.style.height = "50%";
    SearchForEventFullContainer.style.top = "50%";
    SearchForEventFullContainer.style.left = "15%";
    SearchForEventFullContainer.style.position = "absolute";
    SearchAutoSuggest.getAutoSuggestControlContainer().style.zIndex = 100;
    $(SearchAutoSuggest.getSuggestedValueContainer()).addClass(CurrentTheme.ContentSection);
    SearchAutoSuggest.getInputBox().style.backgroundColor="white"
    $(SearchAutoSuggest.getInputBox()).focus();

    //$(document).mouseup(outsideClick)

    document.addEventListener("mouseup", outsideClick)

    function outsideClick(e)    
    {
        var container = $(SearchAutoSuggest.getAutoSuggestControlContainer());
        if (!container.is(e.target) // if the target of the click isn't the container...
            && container.has(e.target).length === 0) // ... nor a descendant of the container
        {
            outOfFocusInput();
            document.removeEventListener("mouseup", outsideClick);
        }
    }

    //$(SearchAutoSuggest.getAutoSuggestControlContainer()).mouseup(outOfFocusInput);

    function outOfFocusInput()
    {
        $(TopBannerDisablePanel).hide();
        DisablePanel.style.zIndex = 0;
        TopBannerDisablePanel.style.zIndex = 0;
        $(DisablePanel).hide();
        SearchAutoSuggest.getAutoSuggestControlContainer().outerHTML = "";

    }

    return;
}

/*
function generateSearchBarContainer(ParentDom)
{
    var SearchBarContainerID = "SearchBarContainer";
    var SearchBarContainer = getDomOrCreateNew(SearchBarContainerID);
    ParentDom = ParentDom.data[0];
    ParentDom.appendChild(SearchBarContainer.Dom);
    $(ParentDom).addClass("textshadow");
    $(ParentDom).addClass("blurry-text");

    
    //(getDomOrCreateNew("NameProfileInfoContainerContent").Dom).style.textShadow = "0 0 15px rgba(0,0,0,0.5)";
    

    
    var DomAndContainer = generateFullSearchBar();


    var SearchBarAndContentContainerID = "SearchBarAndContentContainer";
    var SearchBarAndContentContainer = getDomOrCreateNew(SearchBarAndContentContainerID);


    SearchBarAndContentContainer.Dom.appendChild(DomAndContainer.SearchBar.Dom);
    SearchBarAndContentContainer.Dom.appendChild(DomAndContainer.returnedValue.Dom);


    SearchBarAndContentContainer.Dom.appendChild(DomAndContainer.SearchBar.Dom);

    SearchBarContainer.Dom.appendChild(SearchBarAndContentContainer.Dom);
}

var CountInput = 0;

function generateFullSearchBar()
{
    var SearchBarID = "SearchBar";
    var SearchBar = getDomOrCreateNew(SearchBarID, "input");

    setTimeout(PrepcreationOfRetvalueContainer(SearchBar.Dom), 50);
    $(SearchBar.Dom).on('input', prepCalToBackEnd(SearchBar.Dom, SearchBar))
    var returnedValueContainerID = "returnedValueContainer";
    var returnedValueContainer = getDomOrCreateNew(returnedValueContainerID);
    
    var retValue = { SearchBar: SearchBar, returnedValue: returnedValueContainer };
    return retValue;
}

function prepCalToBackEnd(InputDom, SearchBar)
{
    return function ()
    {
        var FullLetter = InputDom.value;
        var url = "RootWagTap/time.top?WagCommand=10";
        var NameOfEvent = { NameOfEvent: FullLetter, UserName: UserCredentials.UserName, UserID: UserCredentials.ID };


        $.ajax({
            type: "POST",
            url: url,
            data: NameOfEvent,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: function (response) {
                response = JSON.parse(response);
                var data = response.Content;
                var b = data;
            }
        });



        var returnedValue = getReturnedValueContainer(FullLetter);
        var retValue = { SearchBar: SearchBar, returnedValue: returnedValue }
    }
}

function PrepcreationOfRetvalueContainer(SearchBarDom)
{
    return function ()
    {
        SearchBarDom.style.width = "100%";
    }
}

function getReturnedValueContainer(returnedValues)
{   //creates autosuggest box container
    var returnedValueContainerID = "returnedValueContainer";
    var heighOfDomEachDom = 150;
    var returnedValueContainer = getDomOrCreateNew(returnedValueContainerID);
    $(returnedValueContainer).empty();
    returnedValueContainer.Dom.outterHTML = "";
    returnedValueContainer.Dom.style.height = 0;
    var NumberOfReturnedValues = returnedValues.length;
    if (NumberOfReturnedValues < 1)
    {
        returnedValueContainer.Dom.innerHTML = "";
        $(returnedValueContainer.Dom).hide();
    }
    else
    {
        returnedValueContainer.Dom.innerHTML = "";
        $(returnedValueContainer.Dom).show();
        var HeightOfContainer = heighOfDomEachDom * NumberOfReturnedValues;
        returnedValueContainer.Dom.style.height = HeightOfContainer + "px";
        for (var i = 0; i < returnedValues.length; i++)
        {
            //returnedValues[i];
            var EntryRow = generateRowForReturnedValue(returnedValues[i]);
            returnedValueContainer.Dom.appendChild(EntryRow.Dom);
            EntryRow.Dom.style.height = heighOfDomEachDom + "px";
            EntryRow.Dom.style.lineHeight = ( heighOfDomEachDom) + "px";

            EntryRow.Dom.style.top = (heighOfDomEachDom * i) + "px";
        }
    }
    return returnedValueContainer;
}


function generateRowForReturnedValue(myCalEvent)
{
    var retCalEventID="retCalEvent"+NumberOfDom++;
    var retCalEvent = getDomOrCreateNew(retCalEventID);
    retCalEvent.Dom.innerHTML = myCalEvent;
    $(retCalEvent.Dom).addClass("retCalEvent");
    return retCalEvent;
}

*/