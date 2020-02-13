class Preview {

    constructor(eventId, domContainer) {
        this.UIContainer = domContainer;
        this.processedDay = {};
        this.previewDays = [];
        this.isLoading = false;
        this.previewEventId = eventId;
        this.currentRequests = [];
    }

    _beforePreviewRequest () {
        if(Array.isArray(this.currentRequests) && this.currentRequests.length > 0) {
            let previousRequest = this.currentRequests.shift();
            previousRequest.abort();
        }
        
    }

    _afterPreveiwRequestCompletes () {
        this.currentRequests.pop();
    }

    setAsNow(subEventId) {

    }

    showError(response) {

    }

    editSubEvent() {
        this._beforePreviewRequest();
        let Url = global_refTIlerUrl + "WhatIf/SubEventEdit";
        let currentSubevent = Dictionary_OfSubEvents[this.previewEventId];
        let postData = getSubeventUpdateData(currentSubevent);
        postData.TimeZone = moment.tz.guess();
        preSendRequestWithLocation(postData);


        this._beforePreviewRequest();
        this.show();
        this.startLoading();

        let request = $.ajax({
            type: "POST",
            url: Url,
            data: postData,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: (response) => {
                let previewDays = PreviewDay.convertPreviewResponseToPreviewDays(response.Content);
                this.processPreviewDays(previewDays);    
                this.show();
                this.endLoading();
            },
            error: (err) => {
                this.showError();
                this.endLoading();
            }

        }).done(
            () => {
                this._afterPreveiwRequestCompletes();
            }
        );
    }

    procrastinateEvent() {
        this._beforePreviewRequest();
        let Url = global_refTIlerUrl + "WhatIf/PushedEvent";
        let currentSubevent = Dictionary_OfSubEvents[this.previewEventId];
        let postData = getProcrastinateSingleEventData(currentSubevent);
        postData.TimeZone = moment.tz.guess();
        preSendRequestWithLocation(postData);


        this._beforePreviewRequest();
        this.show();
        this.startLoading();
        let endLoading = this.endLoading;


        

        setTimeout(() => {
            let previewDays = Preview.generateRandomPreviewDays();
            this.processPreviewDays(previewDays);
            this.show();
            this.endLoading();
        }, 500);
        

        // let request = $.ajax({
        //     type: "POST",
        //     url: Url,
        //     data: postData,
        //     // DO NOT SET CONTENT TYPE to json
        //     // contentType: "application/json; charset=utf-8", 
        //     // DataType needs to stay, otherwise the response object
        //     // will be treated as a single string
        //     //dataType: "json",
        //     success: (response) => {
        //         //PreviewDay.processPreviewRequest(response.Content);
        //         let previewDays = PreviewDay.convertPreviewResponseToPreviewDays(response.Content);
        //         this.processPreviewDays(previewDays);    
        //         this.show();
        //         var myContainer = (response);
        //         if (myContainer.Error.code == 0) {
        //             //exitSelectedEventScreen();
        //         }
        //         else {
        //             var NewMessage = myContainer.Error && myContainer.Error.code && myContainer.Error.Message ? myContainer.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
        //             var ExitAfter = {
        //                 ExitNow: true, Delay: 5000
        //             };
        //         }
        //         this.endLoading();
        //     },
        //     error: (err) => {
        //         var myError = err;
        //         var step = "err";
        //         var NewMessage = err.Error && err.Error.code && err.Error.Message ? err.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
        //         var ExitAfter = {
        //             ExitNow: true, Delay: 1000
        //         };
        //         this.endLoading();
        //     }

        // }).done(
        //     () => {
        //         this._afterPreveiwRequestCompletes();
        //     }
        // );
    }

    procrastinateAll() {
        this._beforePreviewRequest();
        let Url = global_refTIlerUrl + "WhatIf/PushedAll";
        let currentSubevent = Dictionary_OfSubEvents[this.previewEventId];
        let postData = getProcrastinateAllData(currentSubevent);
        postData.TimeZone = moment.tz.guess();
        preSendRequestWithLocation(postData);


        this._beforePreviewRequest();
        this.show();
        this.startLoading();
        let endLoading = this.endLoading;

        let request = $.ajax({
            type: "POST",
            url: Url,
            data: postData,
            // DO NOT SET CONTENT TYPE to json
            // contentType: "application/json; charset=utf-8", 
            // DataType needs to stay, otherwise the response object
            // will be treated as a single string
            //dataType: "json",
            success: (response) => {
                //PreviewDay.processPreviewRequest(response.Content);
                let previewDays = PreviewDay.convertPreviewResponseToPreviewDays(response.Content);
                this.processPreviewDays(previewDays);    
                this.show();
                var myContainer = (response);
                if (myContainer.Error.code == 0) {
                    //exitSelectedEventScreen();
                }
                else {
                    var NewMessage = myContainer.Error && myContainer.Error.code && myContainer.Error.Message ? myContainer.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                    var ExitAfter = {
                        ExitNow: true, Delay: 5000
                    };
                }
                this.endLoading();
            },
            error: (err) => {
                var myError = err;
                var step = "err";
                var NewMessage = err.Error && err.Error.code && err.Error.Message ? err.Error.Message : "Ooops Tiler is having issues accessing your schedule. Please try again Later:X";
                var ExitAfter = {
                    ExitNow: true, Delay: 1000
                };
                this.endLoading();
            }

        }).done(
            () => {
                this._afterPreveiwRequestCompletes();
            }
        );
    }

    bestPossibleDay(subEventId) {

    }

    static generateRandomPreviewDays() {
        let today = new PreviewDay();
        today.start = (new Date().setHours(0,0,0,0));
        let tomorrow  = new PreviewDay();
        tomorrow.start = today.start + OneDayInMs
        let dafAfterTomorrow  = new PreviewDay();
        dafAfterTomorrow.start = tomorrow.start + OneDayInMs
        let devDays = [
            today, tomorrow, dafAfterTomorrow
        ];
        devDays.forEach((devDay) => {
            devDay.randomizeDev();
        });

        return devDays;
    }

    startLoading() {
        this.isLoading = true;
        let loadingBarId = "PreviewLoading";
        let loadingBar = getDomOrCreateNew(loadingBarId);
        $(loadingBar).addClass("active")
    }

    endLoading() {
        this.isLoading = false;
        let loadingBarId = "PreviewLoading";
        let loadingBar = getDomOrCreateNew(loadingBarId);
        $(loadingBar).removeClass("active");
    }

    show() {
        $(this.UIContainer.parentNode).addClass('active');// this will be unnecessary. The goal is what ever container is provided as UIContainer, will append our generated preview node as the child node. If there is no container then this defaults to sliding out of the subevent infomration.
        $(this.UIContainer.parentNode).removeClass('inActive');
        let closePreviewButton = getDomOrCreateNew("closePreview");
        closePreviewButton.innerHTML = "Close"
        closePreviewButton.onclick = this.hide.bind(this);
    }

    hide() {
        $(this.UIContainer.parentNode).removeClass('active');// this will be unnecessary. The goal is what ever container is provided as UIContainer, will append our generated preview node as the child node. If there is no container then this defaults to sliding out of the subevent infomration.
        $(this.UIContainer.parentNode).addClass("inActive");
        $(this.UIContainer).empty();
    }

    sleepRendering(previewDay) {
        if(previewDay.sleep) {
            
            let sleepData = this.sleepData;
            if(!sleepData) {
                this.sleepData = {};
                sleepData = this.sleepData;
            }
            let dayStart = previewDay.start;
            let daySleepInfo = sleepData[dayStart];
            if (!daySleepInfo) {
                daySleepInfo = {
                    id: globalCounter()
                };
                sleepData[dayStart] = daySleepInfo;
                let sleepContainerDomId = "day-preview-sleep-container-" + daySleepInfo.id;
                let sleepContainer = getDomOrCreateNew(sleepContainerDomId);
                daySleepInfo.container = sleepContainer;
                $(daySleepInfo.container).addClass("day-preview-sleep-container");

                let sleepColorContainerDomId = "day-preview-sleep-color-container-" + daySleepInfo.id;
                let sleepColorContainerDom = getDomOrCreateNew(sleepColorContainerDomId);
                daySleepInfo.colorContainer = sleepColorContainerDom;
                $(daySleepInfo.colorContainer).addClass("day-preview-sleep-color-container");

                let sleepTextContainerDomId = "day-preview-sleep-text-container-" + daySleepInfo.id;
                let sleepTextContainerDom = getDomOrCreateNew(sleepTextContainerDomId);
                daySleepInfo.TextContainer = sleepTextContainerDom;
                $(daySleepInfo.TextContainer).addClass("day-preview-sleep-text-container");

                let sleepTextDomId = "day-preview-sleep-text-" + daySleepInfo.id;
                let sleepTextDom = getDomOrCreateNew(sleepTextDomId, "span");
                daySleepInfo.TextDom = sleepTextDom;
                $(daySleepInfo.TextDom).addClass("day-preview-sleep-text");
                
                daySleepInfo.TextContainer.Dom.appendChild(daySleepInfo.TextDom);
                daySleepInfo.container.Dom.appendChild(daySleepInfo.colorContainer.Dom);
                daySleepInfo.container.Dom.appendChild(daySleepInfo.TextContainer.Dom);
            }

            // daySleepInfo.TextDom.innerHTML = moment.duration(previewDay.sleep.duration, 'milliseconds').humanize();

            let sleepDuration = previewDay.sleep.duration/OneHourInMs;

            let goodSleep = 6;
            let needMoreSleep = 4;
            let badSleep = 2;

            let goodSleepClass="good-sleep";
            let badSleepClass = "bad-sleep";
            let needMoreSleepClass="need-more-sleep";


            $(daySleepInfo.container).removeClass(goodSleepClass);
            $(daySleepInfo.container).removeClass(badSleepClass);
            $(daySleepInfo.container).removeClass(needMoreSleepClass);

            let sleepClass = goodSleepClass
            if(sleepDuration < goodSleep) {
                if(sleepDuration > badSleep) {
                    sleepClass = needMoreSleepClass
                }
                else {
                    sleepClass = badSleepClass;
                }
            }
            

            $(daySleepInfo.container).addClass(sleepClass);

            daySleepInfo.TextDom.innerHTML = Math.round(sleepDuration);
            
            let retValue = {
                dom: daySleepInfo.container.Dom,
                info: daySleepInfo
            }

            return retValue;
        } else {
            return {
                dom: null,
                info: null
            }
        }
    }

    conflictRendering(previewDay) {
        let dayStart = previewDay.start;
        let conflictData = this.conflictData;
        if(!conflictData) {
            conflictData = {}
            this.conflictData = conflictData;
        }

        let dayConflictInfo = conflictData[dayStart];

        if(!dayConflictInfo) {
            dayConflictInfo = {
                id: globalCounter()
            };

            let dayConflictInfoContainerId = "day-preview-conflict-container-"+dayConflictInfo.id;
            let dayConflictInfoContainer = getDomOrCreateNew(dayConflictInfoContainerId);
            dayConflictInfo.conflictContainer = dayConflictInfoContainer;
            $(dayConflictInfo.conflictContainer).addClass("day-preview-conflict-container");

            let dayConflictInfoIconContainerId = "day-preview-conflict-icon-container-"+dayConflictInfo.id;
            let dayConflictInfoIconContainer = getDomOrCreateNew(dayConflictInfoIconContainerId);
            dayConflictInfo.iconContainer = dayConflictInfoIconContainer;
            $(dayConflictInfo.iconContainer).addClass("day-preview-conflict-icon-container");

            let dayConflictInfoTextContainerId = "day-preview-conflict-text-container-"+dayConflictInfo.id;
            let dayConflictInfoTextContainer = getDomOrCreateNew(dayConflictInfoTextContainerId);
            dayConflictInfo.textContainer = dayConflictInfoTextContainer;
            $(dayConflictInfo.textContainer).addClass("day-preview-conflict-text-container");

            let dayConflictInfoTextId = "day-preview-conflict-text-"+dayConflictInfo.id;
            let dayConflictInfoText = getDomOrCreateNew(dayConflictInfoTextId, "span");
            dayConflictInfo.textDom = dayConflictInfoText;
            $(dayConflictInfo.textDom).addClass("day-preview-conflict-text");

            dayConflictInfoTextContainer.Dom.appendChild(dayConflictInfoText);
            dayConflictInfoContainer.Dom.appendChild(dayConflictInfoIconContainer);
            dayConflictInfoContainer.Dom.appendChild(dayConflictInfoTextContainer);

            conflictData[dayStart] = dayConflictInfo;
        }

        if(previewDay.conflict && previewDay.conflict.length > 0) {
            dayConflictInfo.textDom.innerHTML = previewDay.conflict.length + " conflict" + (previewDay.conflict.length>1 ? "s" :"");
        } else {
            dayConflictInfo.textDom.innerHTML = "No conflicts";
        }
        let retValue = {
            dom: dayConflictInfo.conflictContainer
        }

        return retValue;
        
    }

    tardyRendering(previewDay) {
        let dayStart = previewDay.start;
        let tardyData = this.tardyData;
        if(!tardyData) {
            tardyData = {}
            this.tardyData = tardyData;
        }

        let dayTardyInfo = tardyData[dayStart];

        if(!dayTardyInfo) {
            dayTardyInfo = {
                id: globalCounter()
            };

            let dayTardyInfoContainerId = "day-preview-tardy-container-"+dayTardyInfo.id;
            let dayTardyInfoContainer = getDomOrCreateNew(dayTardyInfoContainerId);
            dayTardyInfo.tardyContainer = dayTardyInfoContainer;
            $(dayTardyInfo.tardyContainer).addClass("day-preview-tardy-container");

            let dayTardyInfoIconContainerId = "day-preview-tardy-icon-container-"+dayTardyInfo.id;
            let dayTardyInfoIconContainer = getDomOrCreateNew(dayTardyInfoIconContainerId);
            dayTardyInfo.iconContainer = dayTardyInfoIconContainer;
            $(dayTardyInfo.iconContainer).addClass("day-preview-tardy-icon-container");

            let dayTardyInfoIconId = "day-preview-tardy-icon-"+dayTardyInfo.id;
            let dayTardyInfoIcon = getDomOrCreateNew(dayTardyInfoIconId);
            dayTardyInfo.icon = dayTardyInfoIcon;
            $(dayTardyInfo.icon).addClass("day-preview-tardy-icon");
            dayTardyInfoIconContainer.appendChild(dayTardyInfoIcon);


            let dayTardyInfoTextContainerId = "day-preview-tardy-text-container-"+dayTardyInfo.id;
            let dayTardyInfoTextContainer = getDomOrCreateNew(dayTardyInfoTextContainerId);
            dayTardyInfo.textContainer = dayTardyInfoTextContainer;
            $(dayTardyInfo.textContainer).addClass("day-preview-tardy-text-container");

            let dayTardyInfoTextId = "day-preview-tardy-text-"+dayTardyInfo.id;
            let dayTardyInfoText = getDomOrCreateNew(dayTardyInfoTextId);
            dayTardyInfo.textDom = dayTardyInfoText;
            $(dayTardyInfo.textDom).addClass("day-preview-tardy-text");

            let dayTardyInfoCountId = "day-preview-tardy-count-"+dayTardyInfo.id;
            let dayTardyInfoCount = getDomOrCreateNew(dayTardyInfoCountId);
            dayTardyInfo.infoCount = dayTardyInfoCount;
            $(dayTardyInfo.infoCount).addClass("day-preview-tardy-count");

            let dayTardyInfoDescId = "day-preview-tardy-desc-"+dayTardyInfo.id;
            let dayTardyInfoDesc = getDomOrCreateNew(dayTardyInfoDescId);
            dayTardyInfo.infoDesc = dayTardyInfoDesc;
            $(dayTardyInfo.infoDesc).addClass("day-preview-tardy-desc");

            dayTardyInfoText.Dom.appendChild(dayTardyInfoCount);
            dayTardyInfoText.Dom.appendChild(dayTardyInfoDesc);


            dayTardyInfoTextContainer.Dom.appendChild(dayTardyInfoText);
            dayTardyInfoContainer.Dom.appendChild(dayTardyInfoIconContainer);
            dayTardyInfoContainer.Dom.appendChild(dayTardyInfoTextContainer);

            tardyData[dayStart] = dayTardyInfo;
        }

        if(previewDay.tardy && previewDay.tardy.length > 0) {
            dayTardyInfo.infoCount.innerHTML = previewDay.tardy.length;
            dayTardyInfo.infoDesc.innerHTML = "Late";
            $(dayTardyInfo.infoCount).removeClass("setAsDisplayNone");
            $(dayTardyInfo.tardyContainer).addClass("not-on-time");
            $(dayTardyInfo.tardyContainer).removeClass("on-time");
        } else {
            dayTardyInfo.infoCount.innerHTML = "";
            $(dayTardyInfo.infoCount).addClass("setAsDisplayNone");
            dayTardyInfo.infoDesc.innerHTML = "On Time";
            $(dayTardyInfo.tardyContainer).addClass("on-time");
            $(dayTardyInfo.tardyContainer).removeClass("not-on-time");
        }
        let retValue = {
            dom: dayTardyInfo.tardyContainer
        }

        return retValue;
    }

    generateDayDom(previewDay) {
        let dayStart = Number(previewDay.start);
        let dayInfo = this.processedDay[dayStart];
        if(!dayInfo) {
            dayInfo = {
                id: globalCounter(),
            };

            let dayContainerId = "preview-day-whole-container-"+dayInfo.id;
            let dayContainer = getDomOrCreateNew(dayContainerId);
            dayInfo.dayContainer = dayContainer;
            $(dayInfo.dayContainer).addClass("preview-day-whole-container");

            let dayOfWeekContainerId = "preview-day-of-week-"+dayInfo.id;
            let dayOfWeekContainer = getDomOrCreateNew(dayOfWeekContainerId);
            dayInfo.dayOfWeekContainer = dayOfWeekContainer;
            $(dayInfo.dayOfWeekContainer).addClass("preview-day-of-week");

            let dayOfWeekTextContainerId = "preview-day-of-week-text-container-"+dayInfo.id;
            let dayOfWeekTextContainer = getDomOrCreateNew(dayOfWeekTextContainerId);
            dayInfo.dayOfWeekTextContainer = dayOfWeekTextContainer;
            $(dayInfo.dayOfWeekTextContainer).addClass("preview-day-of-week-text-container");

            let dayOfWeekTextId = "preview-day-of-week-text-"+dayInfo.id;
            let dayOfWeekText = getDomOrCreateNew(dayOfWeekTextId, 'span');
            $(dayInfo.dayOfWeekTextContainer).addClass("preview-day-of-week-text");
            let dayDate = new Date(dayStart);
            let beginningOfDay =new Date(dayDate.setHours(0,0,0,0));
            let toDayBeginning = new Date().setHours(0,0,0,0);
            let tomorrow = toDayBeginning + OneDayInMs;
            let dayOfWeekString = WeekDays[new Date(dayStart).getDay()];
            dayOfWeekString = dayOfWeekString.substring(0, 3) +"  "+ (new Date(dayStart).getMonth()+1) +"/"+ (new Date(dayStart).getDate()+1)
            if(tomorrow ===  dayStart) {
                dayOfWeekString = "Tomorrow";
            }

            if(toDayBeginning ===  dayStart) {
                dayOfWeekString = "Today";
            }

            dayOfWeekText.innerHTML = dayOfWeekString;
            


            dayInfo.dayOfWeekText = dayOfWeekText;
            dayInfo.dayOfWeekTextContainer.Dom.appendChild(dayInfo.dayOfWeekText);
            dayInfo.dayOfWeekContainer.Dom.appendChild(dayInfo.dayOfWeekTextContainer);

            let previewAttributeWrapperId = "day-preview-attribute-wrapper-"+dayInfo.id;
            let previewAttributeWrapper = getDomOrCreateNew(previewAttributeWrapperId);
            dayInfo.previewAttributeDom = previewAttributeWrapper;

            dayInfo.dayContainer.Dom.appendChild(dayInfo.previewAttributeDom.Dom);
            dayInfo.dayContainer.Dom.appendChild(dayInfo.dayOfWeekContainer);

            this.processedDay[dayStart] = dayInfo;
        }

        let sleepInfo = this.sleepRendering(previewDay);
        let conflictInfo = this.conflictRendering(previewDay);
        let tardyInfo = this.tardyRendering(previewDay);
        if(sleepInfo.dom) {
            dayInfo.previewAttributeDom.Dom.appendChild(sleepInfo.dom);
        }
        dayInfo.previewAttributeDom.Dom.appendChild(tardyInfo.dom);
        dayInfo.previewAttributeDom.Dom.appendChild(conflictInfo.dom);
        return dayInfo.dayContainer.Dom;
    }


    processPreviewDays(previewDays) {
        if(previewDays && Array.isArray(previewDays) && previewDays.length > 0) {
            let dayDoms = [];
            for (let i=0; i< previewDays.length; i++) {
                let previewDay = previewDays[i];
                let dayDom = this.generateDayDom(previewDay);
                dayDoms.push(dayDom);
                this.UIContainer.appendChild(dayDom);
            }
        }
    }
}

class SetAsNowPreview {
    constructor(eventId, domContainer) {
        this.DayEvaluation = {}
    }

    sendRequest(eventId) {
        
    }

    processSleep(day) {
        let dayId = day.Start
        let sleepDomId = "day-preview-sleep-container-" + dayId;
        let sleepDom = getDomOrCreateNew(sleepDomId);
        $(sleepDom).addClass("day-preview-sleep-container");
        let retValue = {day: day, dom: sleepDom};

        return retValue;
    }

    processTardy(day) {
        let dayId = day.Start
        let lateDomId = "day-preview-late-container-" + dayId;
        let lateDom = getDomOrCreateNew(lateDomId);
        let lateCountDomId= "day-preview-late-container-" + dayId;
        let lateCountDom = getDomOrCreateNew(lateCountDomId);
        lateCountDom.Dom.innerHTML = day.late.length;
        lateDom.appendChild(lateCountDom)
        let retValue = {day: day, dom: lateDom};
        return retValue;
    }

    processDayDom (day) {
        let dayId = day.Start
        let dayDomId = "day-preview-container-" + dayId;
        let dayDomContainer = getDomOrCreateNew(dayDomId)
        let sleepInfo = this.procesSleep(day);
        let daySleepWrapperId = "day-preview-sleep-wrapper-" + dayId;
        let daySleepWrapperDom = getDomOrCreateNew(daySleepWrapperId);
        daySleepWrapperDom.Dom.appendChild(sleepInfo.dom);

        let tardyInfo = this.processTardy(day);
        let dayTardyWrapperId = "day-preview-tardy-wrapper-" + dayId;
        let dayTardyWrapperDom = getDomOrCreateNew(dayTardyWrapperId);
        dayTardyWrapperDom.Dom.appendChild(tardyInfo.dom);


        dayDomContainer.Dom.appendChild(daySleepWrapperDom.Dom);
        dayDomContainer.Dom.appendChild(dayTardyWrapperDom.Dom);

        let retValue = {
            dom: dayDomContainer,
            sleep: sleepInfo
        }

        return retValue;
    }



    processWebRequest(data) {
        let days = []
        days.sort((day) => {return day.Start - day.Start})
        days.forEach((day) => {
            this.processDayDom(day)
        })
    }

    update(eventId) {

    }
}
class ProcrastinateAllPreview {
    constructor() {
        
    }

    update(timeSpan) {

    }
}