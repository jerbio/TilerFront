

function DropDownNotification()
{
    this.ShowMessage = function (message)
    {
        DropDownNotification.MessageDropDownContainer.Dom.innerHTML = message;
        DropDownNotification.DropDownContainer.Dom.style.transform = "translateY(0%)"
        setTimeout(HideMessage, 3000);
    }

    function HideMessage()
    {
        DropDownNotification.DropDownContainer.Dom.style.transform = "translateY(-100%)"
        setTimeout(function () {
            DropDownNotification.MessageDropDownContainer.Dom.innerHTML = "";
        }, 1500);
    }

    this.HideMessage = HideMessage;
}


function InitializeDropDownNotification()
{
    DropDownNotification.DropDownContainer = getDomOrCreateNew("NotificationContainer");
    DropDownNotification.MessageDropDownContainer = getDomOrCreateNew("NotificationMessageContainer");
    DropDownNotification.IconContainer = getDomOrCreateNew("NotificationIconContainer");
    DropDownNotification.DropDownContainer.Dom.appendChild(DropDownNotification.MessageDropDownContainer.Dom);
    DropDownNotification.DropDownContainer.Dom.appendChild(DropDownNotification.IconContainer.Dom);
    setTimeout(function () {
        $('body').append(DropDownNotification.DropDownContainer.Dom);
        var QuickNotification = new DropDownNotification();
        //$(DropDownNotification.DropDownContainer.Dom).show();
        QuickNotification.ShowMessage("Current Time is " + new Date().toLocaleTimeString('en-US', { formatMatcher: "basic", hour12: true }));
    }, 3000);
}

InitializeDropDownNotification();

DropDownNotification.Status = false;
DropDownNotification.Counter = 0;