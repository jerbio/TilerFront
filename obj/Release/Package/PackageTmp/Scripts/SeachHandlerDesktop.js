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


    if (!ActivateUserSearch.isSearchOn)
    {
        return;
    }

    if((e.which<46)||(e.which>90))
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
ActivateUserSearch.AutoSuggest = {};
document.onkeydown = ActivateUserSearch;
