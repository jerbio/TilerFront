//Object that is the same as response from server for simple testing

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
        }

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
    
}