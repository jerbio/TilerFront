﻿/*
*Function generates a desired HTML element. The default it a Div. First arguement is the desited ID, returns Null if an ID is not provided. Second arguement is desired DOM type e.g span, label, button etc
*/
function getDomOrCreateNew(DomID, DomType) {
    var retValue = { status: false, Dom: null, Misc: null };
    if ((DomID == null) || (DomID == undefined))//checks domID for valid ID
    {
        return retValue;

    }

    if (DomType == null) {
        DomType = "div";
    }

    var myDom = document.getElementById(DomID);
    retValue.status = true;
    if (myDom == null) {
        myDom = document.createElement(DomType);
        myDom.setAttribute("id", DomID);
        retValue.status = false;
    }

    retValue.Dom = myDom;

    var JustAnotherObj = retValue;
    retValue = JustAnotherObj.Dom;
    retValue.status = JustAnotherObj.status;
    retValue.Dom = JustAnotherObj.Dom;
    retValue.Misc = JustAnotherObj.Misc;
    retValue.DomID = DomID;
    return retValue;
}

function generateUUID() {
    var d = new Date().getTime();
    if (window.performance && typeof window.performance.now === "function") {
        d += performance.now(); //use high-precision timer if available
    }
    var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (d + Math.random() * 16) % 16 | 0;
        d = Math.floor(d / 16);
        return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
    return uuid;
}

function getCalBodyContainer() {
    let retValue = getDomOrCreateNew('CalBodyContainer')
    return retValue
}

/*
function binds the date selector to the click event of the passed "LaunchDOm"
*/
function BindImputToDatePicketMobile(LaunchDOm) {
    LaunchDOm.onclick = function () {
        var Container = getDomOrCreateNew("ContainerDateElement");
        LaunchDatePicker(false, Container.Dom, LaunchDOm);
        Container.Dom.style.display = "block";
        CurrentTheme.getCurrentContainer().appendChild((Container.Dom));
    }
}

// got from http://stackoverflow.com/questions/20817618/is-there-a-splice-method-for-strings
function spliceSlice(str, index, count, add) {
    // We cannot pass negative indexes dirrectly to the 2nd slicing operation.
    if (index < 0) {
        index = str.length + index;
        if (index < 0) {
            index = 0;
        }
    }

    return str.slice(0, index) + (add || "") + str.slice(index + count);
}