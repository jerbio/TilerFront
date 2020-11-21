function renderEventDetails(eventId, dictionaryOfSubEvents, dictionaryOfCalEvents) {
    if (!dictionaryOfSubEvents) {
        dictionaryOfSubEvents = Dictionary_OfSubEvents
    }

    if (!dictionaryOfCalEvents) {
        dictionaryOfCalEvents = Dictionary_OfCalendarData
    }


    if(dictionaryOfSubEvents && dictionaryOfCalEvents && eventId) {
        getSubEvent(eventId)
        .then((subEvent) => {
            renderCalEventDetails(subEvent)
        })
    }
}

function renderCalEventDetails(calEvent) {

}