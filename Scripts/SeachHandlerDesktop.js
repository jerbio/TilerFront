function ActivateUserSearch(e)
{
    var SearchInput = getDomOrCreateNew("SearchBar");
    var SearchContainer = getDomOrCreateNew("SearchBarAndContentContainer");
    if (e.shiftKey || e.ctrlKey || e.altKey)
    {
        return;
    }
    if (e.which == 27)
    {
        if (ActivateUserSearch.isSearchOn)
        {
            $(SearchContainer.Dom).removeClass("FullScreenSearchContainer");
            SearchInput.Dom.value = "";
            ActivateUserSearch.AutoSuggest.clear();
            SearchInput.Dom.blur();
            ActivateUserSearch.isSearchOn = false;
            return;
        }
        else
        {
            return;
        }
    }
    if (ActivateUserSearch.isSearchOn)
    {
        return;
    }

    if((e.which<46)||(e.which>91))
    {
        return;
    }
    var url = global_refTIlerUrl + "CalendarEvent/Name";
    var AutoSuggestSearch = new AutoSuggestControl(url, "GET", CallBackFunctionForReturnedValues, SearchInput.Dom);
    ActivateUserSearch.AutoSuggest = AutoSuggestSearch;
    SearchContainer.Dom.appendChild(AutoSuggestSearch.getAutoSuggestControlContainer());
    SearchInput.Dom.focus();
    ActivateUserSearch.isSearchOn = true;
    $(SearchContainer.Dom).addClass("FullScreenSearchContainer");
    
}


ActivateUserSearch.isSearchOn = false;
ActivateUserSearch.AutoSuggest = {};
document.onkeydown = ActivateUserSearch;