class PreviewDay {
    constructor() {
        this.start = 0;
        this.activeStartTime = 0;
        this.sleep = {};
        this.tardy = [];
        this.conflict = [];
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

        //process conflict info
        for(let dayStart in data.after.conflict.days) {
            let entry = dayToData[dayStart];
            if (!entry) {
                entry = {
                    conflicts: []
                };

                dayToData[dayStart] = entry;
            }

            if(!entry.conflicts) {
                entry.conflicts = [];
            }

            let conflictBlobs = data.after.tardy.days[dayStart];
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
            let entry = dayToData[key];
            if(!entry) {
                entry = {
                    sleep: {}
                };
                dayToData[key] = entry;
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
            let entry = dayToData[key];
            if(!entry) {
                entry = {
                    tardy: []
                };

                dayToData[key] = entry;
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