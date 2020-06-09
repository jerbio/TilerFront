class PreviewDay {
    constructor() {
        this.start = 0;
        this.activeStartTime = 0;
        this.sleep = {};
        this.tardy = [];
        this.conflict = [];
        this.tardyDelta = undefined;
        this.conflictDelta = undefined;
        this.sleepDelta = undefined;
    }

    randomizeDev() {
        let maxHours = 36000000 // 10 hours
        let hours = randomInteger(maxHours);
        let sleepSpan = hours;
        let nonSleepSpan = OneDayInMs - sleepSpan;
        let sleepTime = -1;
        let timeA = this.start + randomInteger(nonSleepSpan);
        let timeB = this.start + randomInteger(nonSleepSpan);

        if(timeA < timeB) {
            this.activeStartTime = timeA;
            sleepTime = timeB;
        } else {
            this.activeStartTime = timeB;
            sleepTime = timeA;
        }

        this.sleep = {
            duration: hours,
            sleepStartTime: sleepTime,
            sleepWakeTime: sleepTime + sleepSpan
        }

        this.conflict = this.generateRandomConflict();

        let maxTardyCount = randomInteger(8);
        for (let i = 0; i< maxTardyCount; i++) {
            let tardy = this.generateRandomTardy();
            this.tardy.push(tardy);
        }
    }

    generateRandomTardy () {
        let maxTardy = 2 * OneHourInMs;
        let tardySpan = randomInteger(maxTardy);
        let name = generateRandomEventName();
        let subeventSpan = randomInteger(OneHourInMs);
        let subEventStart = this.start + randomInteger(OneDayInMs - OneHourInMs);
        let subEventEnd = subEventStart + subeventSpan;
        let retValue = {
            duration: tardySpan,
            SubCalEvent: {
                name: name,
                SubCalStartDate: subEventStart,
                SubCalEndDate: subEventEnd
            }
        };

        return retValue;
    }

    generateRandomConflict () {
        let maxSubEventCount = 10;
        let maxTardy = 2 * OneHourInMs;
        let subeventSpan = randomInteger(OneHourInMs);
        let subEventStart = this.start + randomInteger(OneDayInMs - OneHourInMs);
        let subEventEnd = subEventStart + subeventSpan;
        let retValue = []
        let conflictCount = randomInteger(maxSubEventCount);
        for (let i=0; i<conflictCount;i++) {
            let name = generateRandomEventName();
            let timeLine = generateTimeLine(this.start);
            let subEvent =  {
                name: name,
                SubCalStartDate: timeLine.start,
                SubCalEndDate: timeLine.end
            };
            retValue.push(subEvent);
        }
        
        return retValue;
    }

    static processPreviewRequest(data) {
        let dayToData = {};


        function processAfter() {
            let afterDayToData = {};
            //process conflict info
            for(let dayStart in data.after.conflict.days) {
                let entry = afterDayToData[dayStart];
                if (!entry) {
                    entry = {
                        conflicts: []
                    };

                    afterDayToData[dayStart] = entry;
                }

                if(!entry.conflicts) {
                    entry.conflicts = [];
                }

                let conflictBlobs = data.after.conflict.days[dayStart];
                if(conflictBlobs) {
                    for (let i = 0; i<conflictBlobs.length; i++) {
                        let blobsubEvent =  conflictBlobs[i];
                        let conflictingSubEvents = blobsubEvent.subEventsInBlob;
                        for( let j= 0; j < conflictingSubEvents.length; j++) {
                            let subEvent = conflictingSubEvents[i];
                            entry.conflicts.push(subEvent);
                        }
                    }
                }
                
            }

            //process sleep info
            for(let key in data.after.sleep.days) {
                let entry = afterDayToData[key];
                if(!entry) {
                    entry = {
                        sleep: {}
                    };
                    afterDayToData[key] = entry;
                }
                
                if(!entry.sleep) {
                    entry.sleep = {};
                }

                let SleepTimeline = data.after.sleep.days[key].SleepTimeline;
                entry.sleep.sleepStartTime = SleepTimeline.start;
                entry.sleep.sleepWakeTime = SleepTimeline.end;
                entry.sleep.duration = SleepTimeline.duration;
            }

            //process tardy info
            for(let key in data.after.tardy.days) {
                let entry = afterDayToData[key];
                if(!entry) {
                    entry = {
                        tardy: []
                    };

                    afterDayToData[key] = entry;
                }

                if(!entry.tardy) {
                    entry.tardy = [];
                }

                let tardySubevents = data.after.tardy.days[key];
                if(tardySubevents) {
                    for(let i= 0; i < tardySubevents.length; i++) {
                        let tardySubEvent = tardySubevents[i];
                        let previewTardy = {
                            duration: 0,
                            SubCalEvent: {
                                name: tardySubEvent.name,
                                SubCalStartDate: tardySubEvent.start,
                                SubCalEndDate: tardySubEvent.end
                            }
                        };
                        entry.tardy.push(previewTardy);
                    }
                }
            }

            return afterDayToData;
        }

        function processBefore() {
            let beforeDayToData = {};
            //process conflict info
            for(let dayStart in data.before.conflict.days) {
                let entry = beforeDayToData[dayStart];
                if (!entry) {
                    entry = {
                        conflicts: []
                    };

                    beforeDayToData[dayStart] = entry;
                }

                if(!entry.conflicts) {
                    entry.conflicts = [];
                }

                let conflictBlobs = data.before.conflict.days[dayStart];
                if(conflictBlobs) {
                    for (let i = 0; i<conflictBlobs.length; i++) {
                        let blobsubEvent =  conflictBlobs[i];
                        let conflictingSubEvents = blobsubEvent.subEventsInBlob;
                        for( let j= 0; j < conflictingSubEvents.length; j++) {
                            let subEvent = conflictingSubEvents[i];
                            entry.conflicts.push(subEvent);
                        }
                    }
                }
                
            }

            //process sleep info
            for(let key in data.before.sleep.days) {
                let entry = beforeDayToData[key];
                if(!entry) {
                    entry = {
                        sleep: {}
                    };
                    beforeDayToData[key] = entry;
                }
                
                if(!entry.sleep) {
                    entry.sleep = {};
                }

                let SleepTimeline = data.before.sleep.days[key].SleepTimeline;
                entry.sleep.sleepStartTime = SleepTimeline.start;
                entry.sleep.sleepWakeTime = SleepTimeline.end;
                entry.sleep.duration = SleepTimeline.duration;
            }

            //process tardy info
            for(let key in data.before.tardy.days) {
                let entry = beforeDayToData[key];
                if(!entry) {
                    entry = {
                        tardy: []
                    };

                    beforeDayToData[key] = entry;
                }

                if(!entry.tardy) {
                    entry.tardy = [];
                }

                let tardySubevents = data.before.tardy.days[key];
                if(tardySubevents) {
                    for(let i= 0; i < tardySubevents.length; i++) {
                        let tardySubEvent = tardySubevents[i];
                        let previewTardy = {
                            duration: 0,
                            SubCalEvent: {
                                name: tardySubEvent.name,
                                SubCalStartDate: tardySubEvent.start,
                                SubCalEndDate: tardySubEvent.end
                            }
                        };
                        entry.tardy.push(previewTardy);
                    }
                }
            }

            return beforeDayToData
        }

        let beforeProcessed = processBefore();
        let afterProcessed = processAfter();

        for (let dayIndex in beforeProcessed) {
            let beforeDayData = beforeProcessed[dayIndex];
            let afterDayData = afterProcessed[dayIndex];
            if(!isUndefinedOrNull(afterDayData)) {
                let dayData = {};
                let beforeConflict = beforeDayData.conflicts || [];
                let afterConflict = afterDayData.conflicts || [];
                let conflictDelta = afterConflict.length - beforeConflict.length;

                let beforeSleep = beforeDayData.sleep || {duration: 0};
                let afterSleep = afterDayData.sleep || {duration: 0};
                let sleepDelta = afterSleep.duration - beforeSleep.duration;

                let beforeTardy = beforeDayData.tardy || [];
                let afterTardy = afterDayData.tardy || [];
                let tardyDelta = afterTardy.length - beforeTardy.length;

                
                afterDayData.tardyDelta = tardyDelta;
                afterDayData.conflictDelta = conflictDelta;
                afterDayData.sleepDelta = sleepDelta;
                // dayData.tardy = afterDayData.tardy;
                // dayData.conflicts = afterDayData.conflicts;
                // dayData.sleep = afterDayData.sleep;

                dayToData[dayIndex] = afterDayData;
            }
        }
        
        for (let dayIndex in afterProcessed) {
            let afterDayData = afterProcessed[dayIndex];
            let dayData = dayToData[dayIndex];
            if(isUndefinedOrNull(dayData)) {

                let tardyDelta = (afterDayData.tardy || []).length;
                let conflictDelta = (afterDayData.conflicts || []).length;
                let sleepDelta = afterDayData.duration;
                afterDayData.tardyDelta = tardyDelta;
                afterDayData.conflictDelta = conflictDelta;
                afterDayData.sleepDelta = sleepDelta;
                dayToData[dayIndex] = afterDayData;
            }
        }
        



        return dayToData;
    }

    static convertProcessedRequest(data) {
        let retValue = [];

        for(let dayStart in data) {
            let dayData = data[dayStart]
            let previewDay = new PreviewDay();
            previewDay.start = dayStart;
            previewDay.tardy = dayData.tardy;
            previewDay.sleep = dayData.sleep;
            previewDay.conflict = dayData.conflicts;
            previewDay.tardyDelta = dayData.tardyDelta;
            previewDay.sleepDelta = dayData.sleepDelta;
            previewDay.conflictDelta = dayData.conflictDelta;
            retValue.push(previewDay);
        }

        return retValue;
    }
    

    static convertPreviewResponseToPreviewDays(previewResponse) {
        let processedPreviewData = PreviewDay.processPreviewRequest(previewResponse);
        let retValue = PreviewDay.convertProcessedRequest(processedPreviewData);
        return retValue;
    }
}