

function updateIcon() {
    console.log("did you say something");
    console.log(retrieveUserSchedule);
    function subscriber() {
        console.log("Don't wake me up");
    }
    retrieveUserSchedule.c
    setTimeout(updateIcon, 3000)
    //alert(getRefreshedData)
    chrome.browserAction.setIcon({ path: "img/Icons/logoYellow.png", });
    /*
    chrome.browserAction.setIcon({ path: "icon" + current + ".png" });
    current++;

    if (current > max)
        current = min;*/
}

var logos = ["logo.png", "logoYellow.png"]

function setYellowLogoIcon () {
    chrome.browserAction.setIcon({ path: "img/Icons/logoYellow.png", });
}

function setBlackLogoIcon() {
    chrome.browserAction.setIcon({ path: "img/Icons/logo.png", });
}

retrieveUserSchedule.subscribeToBeforeRefresh(setYellowLogoIcon);
retrieveUserSchedule.subscribeToSuccessfulRefresh(setBlackLogoIcon);
