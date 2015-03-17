function loadSelectedSubEvent(EventID, SelectedEvent)
{
    var domID = "SelectedSubevent";
    
    var DomInformation = getDomOrCreateNew(domID);

    $(DomInformation.Dom).empty();
    $(DomInformation.Dom).addClass("ScreenContainer");
    var DomTopSection = generateSelectedEventLabelDom(SelectedEvent);
    var CompletionMapDom = generateCompletionMap(SelectedEvent);
    var NextEventDom = generateNextEventSelection(SelectedEvent);
    var WeatherSelectionDom = generateWeatherSelection(SelectedEvent);
    var RangeUpdateDom=generateRangeUpdate(SelectedEvent)
    var EventOptionsDom = generateEventOptions(SelectedEvent);

    DomInformation.Dom.appendChild(DomTopSection);
    DomInformation.Dom.appendChild(CompletionMapDom);
    DomInformation.Dom.appendChild(NextEventDom);
    DomInformation.Dom.appendChild(WeatherSelectionDom);
    DomInformation.Dom.appendChild(RangeUpdateDom);
    DomInformation.Dom.appendChild(EventOptionsDom);
    
    CurrentTheme.TransitionNewContainer(DomInformation.Dom);
}


function exitSelectedEventScreen()
{
    var myContainer = (CurrentTheme.getCurrentContainer());
    $(myContainer).empty();
    CurrentTheme.TransitionOldContainer();

    myContainer.outerHTML = "";
}

function generateSelectedEventLabelDom(SelectedEvent)
{
    var LabelSectionDom = getDomOrCreateNew("LabelSectionDom"+SelectedEvent.ID)

        //document.createElement("div");
    $(LabelSectionDom.Dom).addClass("LabelSectionDom");
    $(LabelSectionDom.Dom).addClass(CurrentTheme.ContentSection);
    $(LabelSectionDom.Dom).addClass(CurrentTheme.FontColor);

    var HorizontalLine = InsertHorizontalLine("95%", "2.5%", "98%");
    LabelSectionDom.Dom.appendChild(HorizontalLine);
    var LabelBackButton = getDomOrCreateNew("BackIconSelectedEvent").Dom;
    //$(LabelBackButton).css({ "position": "absolute", "left":"25px","top": "50%", "margin-top": "-50px", "width": "100px", "height": "100px" })
    $(LabelBackButton).addClass("BackIcon");
    LabelSectionDom.Dom.appendChild(LabelBackButton);

    $(LabelBackButton).click(function ()
    {
        exitSelectedEventScreen();
    });



    var LabelDescription = getDomOrCreateNew("LabelDescription" + SelectedEvent.ID);
    $(LabelDescription.Dom).addClass("LabelDescription");
    LabelSectionDom.Dom.appendChild(LabelDescription.Dom);
    

    var LabelDescriptionTop = document.createElement("div");
    $(LabelDescriptionTop).addClass("LabelDescriptionTop");
    LabelDescription.Dom.appendChild(LabelDescriptionTop);
    LabelDescriptionTop.innerHTML = SelectedEvent.Name;

    

    var LabelDescriptionTopName = document.createElement("div");
    $(LabelDescriptionTopName).addClass("LabelDescriptionTopName");
    LabelDescriptionTop.appendChild(LabelDescriptionTopName);
    
    var LabelDescriptionBottom = getDomOrCreateNew("LabelDescriptionBottom" + SelectedEvent.ID)
        //document.createElement("div");
    $(LabelDescriptionBottom.Dom).addClass("LabelDescriptionBottom");
    LabelDescription.Dom.appendChild(LabelDescriptionBottom.Dom);
    var VerticalLine = InsertVerticalLine("95%", "50%", "0%");
    LabelDescriptionBottom.Dom.appendChild(VerticalLine);

    var LabelDescriptionBottomLocation = document.createElement("div");
    $(LabelDescriptionBottomLocation).addClass("LabelDescriptionBottomLocation");
    LabelDescriptionBottom.Dom.appendChild(LabelDescriptionBottomLocation);
    LabelDescriptionBottomLocation.innerHTML = SelectedEvent.SubCalAddressDescription;

    var LabelDescriptionBottomTime = document.createElement("div");
    $(LabelDescriptionBottomTime).addClass("LabelDescriptionBottomTime");
    LabelDescriptionBottom.Dom.appendChild(LabelDescriptionBottomTime);
    LabelDescriptionBottomTime.innerHTML = "" + getTimeStringFromDate(SelectedEvent.SubCalStartDate) + "-" + getTimeStringFromDate(SelectedEvent.SubCalEndDate) + "";
    return LabelSectionDom.Dom;
}



function generateNextEventSelection(SelectedEvent)
{
    var NextEventID = "NextEvent";
    var NextEvent = getDomOrCreateNew(NextEventID);
    $(NextEvent.Dom).addClass("SubEventNonLabelSection");//
    $(NextEvent.Dom).addClass(CurrentTheme.AlternateContentSection)
    $(NextEvent.Dom).addClass(CurrentTheme.AlternateFontColor);
    var VerticalLine = InsertVerticalLine("70%", "25%", "17%",null, true);
    $(NextEvent.Dom).append(VerticalLine);
    var TopDividerDom = getNewDividerDom();
    //$(TopDividerDom).css({ "top": "92%", "z-index": "12" });
    $(NextEvent.Dom).append(TopDividerDom);
    CurrentTheme.getCurrentContainer().appendChild(NextEvent.Dom);



    var LabelNextEventID = "LabelNextEvent"
    var LabelNextEvent = getDomOrCreateNew(LabelNextEventID);
    $(LabelNextEvent.Dom).addClass("SubEventLabelSection");
    LabelNextEvent.Dom.innerHTML = "<p>Time \nTill...</p>"
    $(LabelNextEvent.Dom).addClass(CurrentTheme.AlternateFontColor);
    //Populates splitter of Next event section
    /*var SectionSplitterID = "LabelSectionSplitterNextEvent"
    var LabelSectionSplitter = getDomOrCreateNew(SectionSplitterID);
    $(LabelSectionSplitter.Dom).addClass("SectionSplitter");*/
    //Populates  of Next event COntent
    var ContentNextEventID = "ContentNextEvent"
    var ContentNextEvent = getDomOrCreateNew(ContentNextEventID);
    $(ContentNextEvent.Dom).addClass("SubEventContentSection");

    
    
    
    var nextDate = new Date(SelectedEvent.SubCalStartDate);
    UpdateTimer(nextDate, ContentNextEvent.Dom);
    /*var DayID="DayNext"
    var DayDom = getDomOrCreateNew(DayID);
    var DayNameID = "Days";
    var DayNameDom = getDomOrCreateNew(DayNameID);
    DayNameDom.Dom.innerHTML = "Days";
    var DayTextID = "DayText";
    var DayTextDom = getDomOrCreateNew(DayTextID);
    
    var TotalTime =nextDate-Date.now;
    var TotalDays = TotalTime/OneDayInMs;
    var TotalDaysInt =Math.floor(TotalDays);
    var TimeLeft =TotalDays-TotalDaysInt;
    var TotalHours =TimeLeft/OneHourInMs;
    var TotalHoursInt =  Math.floor(TotalHours);
    var 
    var MsLeft = TotalTime-DaysLeft;
    DayTextDom.Dom.innerHTML = 
    


    var HourID = "HourNext"
    var HourDom = getDomOrCreateNew(HourID);
    var MinID = "MinNext";
    var MinDom = getDomOrCreateNew(MinID);
    var SecondID = "SecondNext";
    var SecondDom = getDomOrCreateNew(SecondID);
    */

    NextEvent.Dom.appendChild(LabelNextEvent.Dom);
    //NextEvent.Dom.appendChild(LabelSectionSplitter.Dom);
    NextEvent.Dom.appendChild(ContentNextEvent.Dom);
    
    return NextEvent.Dom;
}

function generateWeatherSelection(SelectedEvent) {
    var WeatherSubEventID = "WeatherSubEvent";
    var WeatherSubEvent = getDomOrCreateNew(WeatherSubEventID);
    $(WeatherSubEvent.Dom).addClass("SubEventNonLabelSection");
    $(WeatherSubEvent.Dom).addClass(CurrentTheme.ContentSection)
    var VerticalLine = InsertVerticalLine("70%", "25%", "17%");
    $(WeatherSubEvent.Dom).append(VerticalLine);
    var TopDividerDom = getNewDividerDom();
    $(WeatherSubEvent.Dom).append(TopDividerDom);



    
    



    //Populates Label of Next event section
    var LabelWeatherSubEventID = "LabelWeatherSubEvent"
    var LabelWeatherSubEvent = getDomOrCreateNew(LabelWeatherSubEventID);
    $(LabelWeatherSubEvent.Dom).addClass("SubEventLabelSection");
    LabelWeatherSubEvent.Dom.innerHTML = "<p>Weather</p>"
    $(LabelWeatherSubEvent.Dom).addClass(CurrentTheme.FontColor);

    //Populates splitter of Next event section
    /*var SectionSplitterID = "LabelSectionSplitter"
    var LabelSectionSplitter = getDomOrCreateNew(SectionSplitterID);
    $(LabelSectionSplitter.Dom).addClass("SectionSplitter");*/
    //Populates  of Next event COntent
    var ContentWeatherSubEventID = "ContentWeatherSubEvent"
    var ContentWeatherSubEvent = getDomOrCreateNew(ContentWeatherSubEventID);
    $(ContentWeatherSubEvent.Dom).addClass("SubEventContentSection");
    var LocationInfo = SelectedEvent.SubCalEventLong + "," + SelectedEvent.SubCalEventLat;
    if ("574_578" == SelectedEvent.ID)
    {
        var b = 900;
    }
    if (LocationInfo == "0,0")
    {
        LocationInfo = SelectedEvent.SubCalAddress;
    }


    $.simpleWeather({
        location: LocationInfo,
        woeid: '',
        unit: 'f',
        success: function (weather) {
            var SelectedSubEventWeatherContainer = getDomOrCreateNew("WeatherContainer");
            ContentWeatherSubEvent.Dom.appendChild(SelectedSubEventWeatherContainer.Dom);
            
            var SelectedSubEventLocationWeatherIconContainer = getDomOrCreateNew("LocationWeatherIconContainer");
            ContentWeatherSubEvent.Dom.appendChild(SelectedSubEventLocationWeatherIconContainer.Dom);
            var SelectedSubEventLocationWeatherIcon = getDomOrCreateNew("LocationWeatherIcon");
            

            $(SelectedSubEventLocationWeatherIconContainer.Dom).append(SelectedSubEventLocationWeatherIcon.Dom);
            var WeatherIndexData = WeatherIndex[getWeatherImageClass(weather.code)]
            $(SelectedSubEventLocationWeatherIcon.Dom).addClass(WeatherIndexData);
            $(SelectedSubEventLocationWeatherIcon.Dom).addClass("LocationWeatherIcon");

            setTimeout(function () {
                SelectedSubEventLocationWeatherIcon.Dom.style.opacity = 1;
                SelectedSubEventLocationWeatherIcon.Dom.style.left = "25%";
            }, 500);
            

            var SelectedSubEventLocationWeatherDataContainer = getDomOrCreateNew("LocationWeatherDataContainer");
            $(SelectedSubEventLocationWeatherDataContainer.Dom).addClass("LocationWeatherDataContainer");
            $(SelectedSubEventLocationWeatherIconContainer.Dom).append(SelectedSubEventLocationWeatherDataContainer.Dom);
            var SelectedSubEventLocationWeatherDataTemp = getDomOrCreateNew("LocationWeatherDataTemp");
            $(SelectedSubEventLocationWeatherDataTemp.Dom).addClass("LocationWeatherDataTemp");
            SelectedSubEventLocationWeatherDataContainer.Dom.appendChild(SelectedSubEventLocationWeatherDataTemp.Dom);
            $(SelectedSubEventLocationWeatherDataContainer.Dom).addClass(CurrentTheme.FontColor);
            $(SelectedSubEventLocationWeatherDataContainer.Dom).addClass

            SelectedSubEventLocationWeatherDataTemp.Dom.innerHTML = weather.temp + '&deg;' + weather.units.temp;

            var SelectedSubEventLocationWeatherDataNonTempData = getDomOrCreateNew("LocationWeatherDataNonTempData");
            $(SelectedSubEventLocationWeatherDataNonTempData.Dom).addClass("LocationWeatherDataNonTempData")
            
            SelectedSubEventLocationWeatherDataContainer.Dom.appendChild(SelectedSubEventLocationWeatherDataNonTempData.Dom);

            $(SelectedSubEventLocationWeatherDataNonTempData.Dom).addClass("LocationWeatherDataTemp");

            var SelectedSubEventLocationWeatherDataWind = getDomOrCreateNew("LocationWeatherDataWind");
            SelectedSubEventLocationWeatherDataWind.Dom.innerHTML =  weather.wind.direction + ' ' + weather.wind.speed + ' ' + weather.units.speed;
            SelectedSubEventLocationWeatherDataNonTempData.Dom.appendChild(SelectedSubEventLocationWeatherDataWind.Dom);

            var SelectedSubEventLocationWeatherDataCity = getDomOrCreateNew("LocationWeatherDataCity");
            SelectedSubEventLocationWeatherDataCity.Dom.innerHTML = weather.city + ", " + weather.region;
            SelectedSubEventLocationWeatherDataNonTempData.Dom.appendChild(SelectedSubEventLocationWeatherDataCity.Dom);


            /*var SelectedSubEventLocationWeatherDataRegion = getDomOrCreateNew("LocationWeatherDataRegion");
            SelectedSubEventLocationWeatherDataRegion.Dom.innerHTML = weather.region;
            SelectedSubEventLocationWeatherDataNonTempData.Dom.appendChild(SelectedSubEventLocationWeatherDataRegion.Dom);*/


            var SelectedSubEventLocationWeatherDataCurrent = getDomOrCreateNew("LocationWeatherDataCurrent");
            SelectedSubEventLocationWeatherDataCurrent.Dom.innerHTML = weather.currently;
            SelectedSubEventLocationWeatherDataNonTempData.Dom.appendChild(SelectedSubEventLocationWeatherDataCurrent.Dom);


            /*html = '<h2><i class="icon-' + weather.code + '"></i> ' + weather.temp + '&deg;' + weather.units.temp + '</h2>';
            html += '<ul><li>' + weather.city + ', ' + weather.region + '</li>';
            html += '<li class="currently">' + weather.currently + '</li>';
            html += '<li>' + weather.wind.direction + ' ' + weather.wind.speed + ' ' + weather.units.speed + '</li></ul>';
            //alert(weather.city);
            $(ContentWeatherSubEvent.Dom).html(html);*/
        },
        error: function (error) {
            var bb = 99;
            //$("#weather").html('<p>' + error + '</p>');
        }
    });

    WeatherSubEvent.Dom.appendChild(LabelWeatherSubEvent.Dom);
    //WeatherSubEvent.Dom.appendChild(LabelSectionSplitter.Dom);
    WeatherSubEvent.Dom.appendChild(ContentWeatherSubEvent.Dom);


    return WeatherSubEvent.Dom;
}

WeatherIndex = { 0: "Cloudy", 1: "Fog", 2: "Hail", 3: "Moon", 4: "PartlySunny", 5: "Rainy", 6: "Snow", 7: "Sunny", 8: "Thunderstorm", 9: "Unknown", 10: "Whirl", 11: "ClearNight", 12: "CloudyNight", 13: "Eclipse", 14: "PartlyCloudy", 15: "Rainbow", 16: "Storm", 17: "Tornado", 18: "Windy" };

function generateRangeUpdate(SelectedEvent) {
    //return;
    var RangeModifyID = "RangeUpdate";
    var RangeModify = getDomOrCreateNew(RangeModifyID);
    $(RangeModify.Dom).addClass("SubEventNonLabelSection");

    $(RangeModify.Dom).addClass(CurrentTheme.AlternateContentSection)
    $(RangeModify.Dom).addClass(CurrentTheme.AlternateFontColor);
    var VerticalLine = InsertVerticalLine("70%", "25%", "17%", null, true);
    $(RangeModify.Dom).append(VerticalLine);


    var TopDividerDom = getNewDividerDom();
    $(RangeModify.Dom).append(TopDividerDom);
    //Populates Label of Next event section
    var LabelRangeModifyID = "LabelRangeModify"
    var LabelRangeModify = getDomOrCreateNew(LabelRangeModifyID);
    $(LabelRangeModify.Dom).addClass("SubEventLabelSection");



    //var LabelWeatherSubEventID = "LabelWeatherSubEvent"
    //var LabelWeatherSubEvent = getDomOrCreateNew(LabelWeatherSubEventID);
    //$(LabelWeatherSubEvent.Dom).addClass("SubEventLabelSection");
    LabelRangeModify.Dom.innerHTML = "<p>Concentrate Date</p>"
    $(LabelRangeModify.Dom).addClass(CurrentTheme.AlternateFontColor);

    //Populates splitter of Next event section
    var SectionSplitterID = "LabelSectionSplitterRangeModify"
    var LabelSectionSplitter = getDomOrCreateNew(SectionSplitterID);
    $(LabelSectionSplitter.Dom).addClass("SectionSplitter");
    //Populates  of Next event COntent
    var ContentRangeModifyID = "ContentRangeModify"
    var ContentRangeModify = getDomOrCreateNew(ContentRangeModifyID);
    $(ContentRangeModify.Dom).addClass("SubEventContentSection");
    RangeModify.Dom.appendChild(LabelRangeModify.Dom);
    //RangeModify.Dom.appendChild(LabelSectionSplitter.Dom);
    RangeModify.Dom.appendChild(ContentRangeModify.Dom);
    //alert(SelectedEvent.SubCalCalEventStart);
    var DateString = SelectedEvent.SubCalCalEventStart;
    var msDate = Date.parse(DateString);
    var StartCalEvent = msDate;
    DateString = SelectedEvent.SubCalCalEventEnd;
    msDate = Date.parse(DateString);
    var EndCalEvent = msDate;
    msDate = Date.parse(SelectedEvent.SubCalStartDate);
    var CurrentDate = msDate;// new Date(SelectedEvent.SubCalStartDate) //- new Date();
    //LabelRangeModify.Dom.innerHTML = CurrentDate.toString();
    var SliderContainer = getDomOrCreateNew("SelectedSubEventRangeSliderContainer");
    var SubEventEndDate = getDomOrCreateNew("SelectedSubEventEndDateContainer");
    var SubEventEndDateName = getDomOrCreateNew("SelectedSubEventEndDateContainerName");
    SubEventEndDateName.Dom.innerHTML="End Date:"
    SubEventEndDate.Dom.appendChild(SubEventEndDateName.Dom);
    var SubEventEndDateNameContent = getDomOrCreateNew("SelectedSubEventEndDateContainerContent");
    SubEventEndDate.Dom.appendChild(SubEventEndDateNameContent.Dom);


    function UpdateDateDisplay() {
        /*var myDate = $(SliderContainer.Dom).slider("value");
        SubEventEndDateNameContent.Dom.innerHTML = DateTimeToDayMDYTimeString(new Date(myDate));*/
    }

    
    

    ContentRangeModify.Dom.appendChild(SliderContainer.Dom);
    ContentRangeModify.Dom.appendChild(SubEventEndDate.Dom);
    
    $(
        function ()
        {
            /*
            $(SliderContainer.Dom).slider({
                    range: true,
                    min: StartCalEvent,
                    max: EndCalEvent,
                    values: CurrentDate,
                    slide: UpdateDateDisplay,
                    change: UpdateDateDisplay
                });
            $(SliderContainer.Dom).slider("value", new Date(CurrentDate));*/
        }
    );
    



    return RangeModify.Dom;
}

function generateEventOptions(SelectedEvent) {
    var EventOptionsID = "EventOptions";
    var EventOptionsSubEvent = getDomOrCreateNew(EventOptionsID);
    $(EventOptionsSubEvent.Dom).addClass("SubEventNonLabelSection");
    $(EventOptionsSubEvent.Dom).addClass(CurrentTheme.ContentSection);

    var TopDividerDom = getNewDividerDom();
    //$(TopDividerDom).css({ "top": "92%", "z-index": "12" });
    $(EventOptionsSubEvent.Dom).append(TopDividerDom);
    CurrentTheme.getCurrentContainer().appendChild(EventOptionsSubEvent.Dom);


    /*
    //Populates Label of Next event section
    var LabelEventOptionsSubEventID = "LabelEventOptionsSubEvent"
    var LabelEventOptionsSubEvent = getDomOrCreateNew(LabelEventOptionsSubEventID);
    $(LabelEventOptionsSubEvent.Dom).addClass("SubEventLabelSection");
    //Populates splitter of Next event section
    var SectionSplitterID = "LabelSectionSplitterEventOptionsSubEvent"
    var LabelSectionSplitter = getDomOrCreateNew(SectionSplitterID);
    $(LabelSectionSplitter.Dom).addClass("SectionSplitter");
    //Populates  of Next event COntent
    var ContentEventOptionsSubEventID = "ContentEventOptionsSubEvent"
    var ContentEventOptionsSubEvent = getDomOrCreateNew(ContentEventOptionsSubEventID);
    $(ContentEventOptionsSubEvent.Dom).addClass("SubEventContentSection");


    */
    /*Populate Options*/
    var ProcrastinateOptionID = "ProcrastinateOption";
    var ProcrastinateOption = getDomOrCreateNew(ProcrastinateOptionID);
    $(ProcrastinateOption.Dom).addClass("SelectedEventOption");
    
    var ProcrastinateOptionImgContainerID = "ProcrastinateOptionImgContainer";
    var ProcrastinateOptionImgContainer = getDomOrCreateNew(ProcrastinateOptionImgContainerID);
    $(ProcrastinateOptionImgContainer.Dom).addClass("SelectedEventOptionImageContainer");
    var ProcrastinateOptionImgID = "ProcrastinateOptionImg";
    var ProcrastinateOptionImg = getDomOrCreateNew(ProcrastinateOptionImgID);

    $(ProcrastinateOptionImg.Dom).addClass("ProcrastinateIcon");
    $(ProcrastinateOptionImg.Dom).addClass("SelectedEventOptionImage");
    ProcrastinateOptionImgContainer.Dom.appendChild(ProcrastinateOptionImg.Dom)

    var ProcrastinateOptionTxtID = "ProcrastinateOptionTxt";
    var ProcrastinateOptionTxt = getDomOrCreateNew(ProcrastinateOptionTxtID);
    $(ProcrastinateOptionTxt.Dom).addClass("SelectedEventOptionText");
    ProcrastinateOption.Dom.appendChild(ProcrastinateOptionImgContainer.Dom);
    ProcrastinateOption.Dom.appendChild(ProcrastinateOptionTxt.Dom);

    $(ProcrastinateOption.Dom).click(function () {

        var myEventID = SelectedEvent.ID;
        ProcrastinateOnEvent(myEventID, EventOptionsSubEvent.Dom);
    });


    

    


    var SilentOptionID = "SilentOption";
    var SilentOption = getDomOrCreateNew(SilentOptionID);
    $(SilentOption.Dom).addClass("SelectedEventOption");
    var SilentOptionImgContainerID = "SilentOptionImgContainer";
    var SilentOptionImgContainer = getDomOrCreateNew(SilentOptionImgContainerID);
    $(SilentOptionImgContainer.Dom).addClass("SelectedEventOptionImageContainer");
    var SilentOptionImgID = "SilentOptionImg";
    var SilentOptionImg = getDomOrCreateNew(SilentOptionImgID);
    $(SilentOptionImg.Dom).addClass("SilentIcon");
    $(SilentOptionImg.Dom).addClass("SelectedEventOptionImage");
    SilentOptionImgContainer.Dom.appendChild(SilentOptionImg.Dom)
    var SilentOptionTxtID = "SilentOptionTxt";
    var SilentOptionTxt = getDomOrCreateNew(SilentOptionTxtID);
    $(SilentOptionTxt.Dom).addClass("SelectedEventOptionText");
    SilentOption.Dom.appendChild(SilentOptionImgContainer.Dom);
    SilentOption.Dom.appendChild(SilentOptionTxt.Dom);
    $(SilentOption.Dom).click(prepFunctionForSilentClick(SelectedEvent.ID));



    var DirectionsOptionID = "DirectionsOption";
    var DirectionsOption = getDomOrCreateNew(DirectionsOptionID);
    $(DirectionsOption.Dom).addClass("SelectedEventOption");
    var DirectionsOptionImgContainerID = "DirectionsOptionImgContainer";
    var DirectionsOptionImgContainer = getDomOrCreateNew(DirectionsOptionImgContainerID);
    $(DirectionsOptionImgContainer.Dom).addClass("SelectedEventOptionImageContainer");
    var DirectionsOptionImgID = "DirectionsOptionImg";
    var DirectionsOptionImg = getDomOrCreateNew(DirectionsOptionImgID);
    $(DirectionsOptionImg.Dom).addClass("DirectionsIcon");
    $(DirectionsOptionImg.Dom).addClass("SelectedEventOptionImage");
    DirectionsOptionImgContainer.Dom.appendChild(DirectionsOptionImg.Dom)
    var DirectionsOptionTxtID = "DirectionsOptionTxt";
    var DirectionsOptionTxt = getDomOrCreateNew(DirectionsOptionTxtID);
    $(DirectionsOptionTxt.Dom).addClass("SelectedEventOptionText");
    DirectionsOption.Dom.appendChild(DirectionsOptionImgContainer.Dom);
    DirectionsOption.Dom.appendChild(DirectionsOptionTxt.Dom);

    $(DirectionsOption.Dom).click(getDirectionsCallBack(SelectedEvent.ID, CurrentTheme));


    

    var MoreOptionID = "MoreOption";
    var MoreOption = getDomOrCreateNew(MoreOptionID);
    $(MoreOption.Dom).addClass("SelectedEventOption");
    var MoreOptionImgContainerID = "MoreOptionImgContainer";
    var MoreOptionImgContainer = getDomOrCreateNew(MoreOptionImgContainerID);
    $(MoreOptionImgContainer.Dom).addClass("SelectedEventOptionImageContainer");
    var MoreOptionImgID = "MoreOptionImg";
    var MoreOptionImg = getDomOrCreateNew(MoreOptionImgID);
    $(MoreOptionImg.Dom).addClass("MoreIcon");
    $(MoreOptionImg.Dom).addClass("SelectedEventOptionImage");
    MoreOptionImgContainer.Dom.appendChild(MoreOptionImg.Dom)
    var MoreOptionTxtID = "MoreOptionTxt";
    var MoreOptionTxt = getDomOrCreateNew(MoreOptionTxtID);
    $(MoreOptionTxt.Dom).addClass("SelectedEventOptionText");
    MoreOption.Dom.appendChild(MoreOptionImgContainer.Dom);
    MoreOption.Dom.appendChild(MoreOptionTxt.Dom);





    EventOptionsSubEvent.Dom.appendChild(ProcrastinateOption.Dom);
    EventOptionsSubEvent.Dom.appendChild(DirectionsOption.Dom);
    EventOptionsSubEvent.Dom.appendChild(SilentOption.Dom);
    EventOptionsSubEvent.Dom.appendChild(MoreOption.Dom);


    return EventOptionsSubEvent.Dom;
}

function prepFunctionForSilentClick(EventID)
{
    return function ()
    {
        /*
        var TimeZone = new Date().getTimezoneOffset();

        
        var MarkAsCompleteData = { UserName: UserCredentials.UserName, UserID: UserCredentials.ID, EventID: EventID ,TimeZoneOffset: TimeZone};
        $.ajax({
            type: "POST",
            url: "RootWagTap/time.top?WagCommand=7",
            data: MarkAsCompleteData,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: function (response) {
                //alert(response);
                var myContainer = JSON.parse(response);
                if (myContainer.Error.code === "0")
                {
                    //exitSelectedEventScreen();
                }
                else {
                    alert("error detected with marking as complete");
                }

            },
            error: function (err) {
                var myError = err;
                var step = "err";
            }

        }).done(function (data) {
            InitializeHomePage();//hack alert
        });*/
    }
}

function UpdateTimer(CountDownDate, EncasingDom)
{
    var DomObject = EncasingDom;//RetrieveDom("Event_Content_Text_Container1_Element1");
    DomObject.innerHTML = "";
    //DomObject.style.backgroundImage = "url('./Images/Praise_Night_Mast.jpg')";
    //DomObject.style.width="90%";
    //DomObject.style.height = "90%";
    //DomObject.style.left = "5%";
    //DomObject.style.top= "5%";
    //DomObject.style.border = "3px solid rgba(100,100,100,1)"
    //DomObject.style.boxShadow = "0px 0px 4px 4px rgba(5,5,5,0.6)"
    //DomObject.style.borderRadius = "5px";

    var DomObject1 = getDomOrCreateNew("Event_Content_Text_Container1_Element_Timer1_Hook_Case1");
    var DomObject2 = getDomOrCreateNew("Event_Content_Text_Container1_Element_Timer1_Hook1");
    DomObject1.Dom.appendChild(DomObject2.Dom);
    var DomObject3 = getDomOrCreateNew("Event_Content_Text_Container1_Element_Timer1");
    EncasingDom.appendChild(DomObject1.Dom);
    EncasingDom.appendChild(DomObject3.Dom);

    //DomObject.innerHTML += "<div class=\"Event_Content_Text_Container1_Element_Timer1_Hook_Case\" id=\"Event_Content_Text_Container1_Element_Timer1_Hook_Case1\"><div class=\"Event_Content_Text_Container1_Element_Timer1_Hook\"id=\"Event_Content_Text_Container1_Element_Timer1_Hook1\"></div></div><div class=\"Event_Content_Text_Container1_Element_Timer\" id=\"Event_Content_Text_Container1_Element_Timer1\"></div>";
    var TimerDomObject = DomObject3.Dom;//RetrieveDom("Event_Content_Text_Container1_Element_Timer1");
    TimerDomObject.style.position = "absolute"
    TimerDomObject.style.top = "10%";
    TimerDomObject.style.height = "80%";
    TimerDomObject.style.width = "80%";
    TimerDomObject.style.left = "10%";
    TimerDomObject.style.backgroundColor = "rgba(119, 3, 94,0)";
    
    TimerDomObject.style.boxShadow = "0px 0px 4px 2px rgba(20,20,20,0)";
    var TimerHookCaseDomObject = DomObject1.Dom;//RetrieveDom("Event_Content_Text_Container1_Element_Timer1_Hook_Case1");
    TimerHookCaseDomObject.style.position = "absolute"
    TimerHookCaseDomObject.style.top="45%";
    TimerHookCaseDomObject.style.height="24px";
    TimerHookCaseDomObject.style.width="80%";
    TimerHookCaseDomObject.style.left="23%";
    TimerHookCaseDomObject.style.marginTop="-24px";
    TimerHookCaseDomObject.style.backgroundColor="transparent";
    TimerHookCaseDomObject.style.overflow = "hidden";
    var TimerHookDomObject = DomObject2.Dom;//RetrieveDom("Event_Content_Text_Container1_Element_Timer1_Hook1");
    TimerHookDomObject.style.position = "absolute"
    TimerHookDomObject.style.left = "100%";
    TimerHookDomObject.style.marginLeft = "-24px";
    TimerHookDomObject.style.border = "24px solid rgba(255, 128, 0,0)";
    TimerHookDomObject.style.zIndex = "-1";
    TimerHookDomObject.style.borderLeft = "24px solid rgba(79, 0, 54,1)";
    CountDownTimer(DomObject3.Dom, CountDownDate);
    //CountDownTimer("Event_Content_Text_Container1_Element_Timer1", CountDownDate) 
}

//function takes two variables 1-DomObject(DomID or Object) to which the counter will be implemented; 2-FinalDate to which you are executing the time difference.
function CountDownTimer(DomObjectEntry, FinalDate)
{
    var DomObject = DomObjectEntry;//RetrieveDom(DomObjectEntry);
    var today = new Date();
    var dd = today.getDate();
    //today.setMonth((today.getMonth()) + 1); //January is 0!
    var milliTimeSpan = FinalDate - today//gets Time Difference in milliseconds
    var SecondsTimeSpan = parseInt(milliTimeSpan / 1000); //converts to milliseconds
    //alert(SecondsTimeSpan)
    var Days = parseInt(SecondsTimeSpan / (60 * 60 * 24));//gets day Value
    var Hours = parseInt((SecondsTimeSpan % (60 * 60 * 24)) / (60 * 60)); //gets remainder hour value 
    var Minutes = parseInt(((SecondsTimeSpan % (60 * 60 * 24)) % (60 * 60))/60);
    var Seconds = parseInt((((SecondsTimeSpan % (60 * 60 * 24)) % (60 * 60)) % (60)));
    UpdateTimerDiv([Days, Hours, Minutes, Seconds], DomObject)
    
    setTimeout(function(){CountDownTimer(DomObjectEntry, FinalDate);},1000)

    function UpdateTimerDiv(TimeData,DomObjectEntry)
    {
        var DayString = "<div class=\"DayString TimeElementString" + CurrentTheme.Color + "\"id=\"DayString1\">" + TimeData[0] + "<div class=\"TimeElementText\" id=\"TimeElementText1\" >Days</div></div>"
        var ColonString1 = "<div class=\"ColonString \"id=\"ColonString1\" ></div>"
        var HourString = "<div class=\"HourString TimeElementString" + CurrentTheme.Color + "\"id=\"HourString1\" >" + TimeData[1] + "<div class=\"TimeElementText\" id=\"TimeElementText2\" >Hours</div></div>"
        var ColonString2 = "<div class=\"ColonString \"id=\"ColonString2\"></div>";
        var MinuteString = "<div class=\"MinuteString TimeElementString" + CurrentTheme.Color + "\"id=\"MinuteString1\">" + TimeData[2] + "<div class=\"TimeElementText\" id=\"TimeElementText3\" >Minutes</div></div>"
        var ColonString3 = "<div class=\"ColonString \"id=\"ColonString3\"></div>";
        var SecondString = "<div class=\"SecondString TimeElementString" + CurrentTheme.Color + "\"id=\"SecondString1\" >" + TimeData[3] + "<div class=\"TimeElementText\" id=\"TimeElementText4\" >Seconds</div></div>"
        DomObjectEntry.innerHTML = DayString + ColonString1 + HourString + ColonString2 + MinuteString + ColonString3 + SecondString;
    }
}

function getWeatherImageClass(WeatherCode) {
    var RetValue = "none"
    switch (WeatherCode) {
        case "0":
            {
                RetValue = 17;
            }
            break;
        case "1":
            {
                RetValue = 16
            }
            break;
        case "2":
            {
                RetValue = 17
            }
            break;
        case "3":
            {
                RetValue = 8;
            }
            break;
        case "4":
            {
                RetValue = 8;
            }
            break;
        case "5":
            {
                RetValue = 6;
            }
            break;
        case "6":
            {
                RetValue = 5;
            }
            break;
        case "7":
            {
                RetValue = 6;
            }
            break;
        case "8":
            {
                RetValue = 6;
            }
            break;
        case "9":
            {
                RetValue = 5;
            }
            break;

        case "10":
            {
                RetValue = 5;
            }
            break;
        case "11":
            {
                RetValue = 5;
            }
            break;
        case "12":
            {
                RetValue = 5;
            }
            break;
        case "13":
            {
                RetValue = 6;
            }
            break;
        case "14":
            {
                RetValue = 6;
            }
            break;
        case "15":
            {
                RetValue = 6;
            }
            break;
        case "16":
            {
                RetValue = 6;
            }
            break;
        case "17":
            {
                RetValue = 2;
            }
            break;
        case "18":
            {
                RetValue = 6;
            }
            break;
        case "19":
            {
                RetValue = 17;
            }
            break;
        case "20":
            {
                RetValue = 1;
            }
            break;
        case "21":
            {
                RetValue = 1;
            }
            break;
        case "22":
            {
                RetValue = 1;
            }
            break;
        case "23":
            {
                RetValue = 1;
            }
            break;
        case "24":
            {
                RetValue = 18;
            }
            break;
        case "25":
            {
                RetValue = 16;
            }
            break;
        case "26":
            {
                RetValue = 0;
            }
            break;
        case "27":
            {
                RetValue = 12;
            }
            break;
        case "28":
            {
                RetValue = 0;
            }
            break;
        case "29":
            {
                RetValue = 14;
            }
            break;
        case "30":
            {
                RetValue = 14;
            }
            break;
        case "31":
            {
                RetValue = 3;
            }
            break;
        case "32":
            {
                RetValue = 7;
            }
            break;
        case "33":
            {
                RetValue = 3;
            }
            break;
        case "34":
            {
                RetValue = 4;
            }
            break;
        case "35":
            {
                RetValue = 3;
            }
            break;
        case "36":
            {
                RetValue = 7;
            }
            break;
        case "37":
            {
                RetValue = 8;
            }
            break;
        case "38":
            {
                RetValue = 8;
            }
            break;
        case "39":
            {
                RetValue = 8;
            }
            break;
        case "40":
            {
                RetValue = 5;
            }
            break;
        case "41":
            {
                RetValue = 6;
            }
            break;
        case "42":
            {
                RetValue = 6;
            }
            break;
        case "43":
            {
                RetValue = 6;
            }
            break;
        case "44":
            {
                RetValue = 14;
            }
            break;
        case "45":
            {
                RetValue = 8;
            }
            break;
        case "46":
            {
                RetValue = 6;
            }
            break;
        case "47":
            {
                RetValue = 5;
            }
            break;
        case "3200":
            {
                RetValue = 9;
            }
            break;
        default:
            {
                RetValue = 9;
            }
    }

    return RetValue;
}