

function LaunchMapInformation(ThemeInformation, EventID)
{
    var mobileOS = ThemeInformation.getMobileOS();
    var LocationData = getLocation(EventID);
    var DirectionsControl;
    switch (mobileOS.toLowerCase())
    {
        case "ios":
            {
                DirectionsControl = new LaunchAppleMap(LocationData);
            }
            break;
        case "android":
            {
                DirectionsControl=new LaunchAndroidMap(LocationData);
            }
            break;
        default:
            {
                DirectionsControl = new LaunchGoogleMaps(LocationData);
            }
            break;
    }
    DirectionsControl.LaunchDirection();
}

function LaunchDirectionInformation(EventID, mobileOS)
{
    
}

function getLocation(EventID)
{
    var LocationData = Dictionary_OfSubEvents[EventID].SubCalAddress;
    return LocationData;
}



function LaunchPlatformMap(LocationData)
{
    this.LocationData = LocationData;
    this.getAddressInformation = getAddressInformaiton;
    this.isLongLatData = isLongLatData;
    this.LauncnInvalidData = LaunchInvalidLocation;
    this.LaunchDirection = LaunchDirection;
    this.LaunchMap = LaunchMap;
    this.getURLString;
    this.StartString;
    this.destinationString;
    this.FullURL;
    function getAddressInformaiton()
    {
        return LocationData.Address;
    }

    function isLongLatData()
    {
        
    }

    function LaunchMap()
    {

    }

    function LaunchDirection()
    {

    }

    function LaunchInvalidLocation()
    {

    }
}

function LaunchAppleMap(LocationData)//handles IOS devices
{
    this.LaunchDirection = LaunchDirection;
    
    this.StartString;
    function LaunchDirection()
    {
        this.destinationString = LocationData;
        this.StartString = "current location";
        this.FullURL = "http://maps.apple.com/?daddr=" + this.destinationString + "&saddr=" + this.StartString;
        window.location = this.FullURL;
    }
}

LaunchAppleMap.prototype = new LaunchPlatformMap;

function LaunchAndroidMap(LocationData)//handles Android devices
{
    this.LaunchDirection = LaunchDirection;
    function LaunchDirection()
    {
        this.StartString = "your location";
        this.destinationString = LocationData;
        this.FullURL = "http://maps.google.com/maps?saddr=" + this.StartString + "&daddr=" + this.destinationString;
        window.location = this.FullURL;
    }
}

LaunchAndroidMap.prototype = new LaunchPlatformMap;

function LaunchWindowsMap(LocationData)//handles Windows devices
{
    function LaunchDirection()
    {
        this.destinationString = LocationData;
    }
}

LaunchWindowsMap.prototype = new LaunchPlatformMap;

function LaunchGoogleMaps(LocationData)//forces to google maps
{
    this.LaunchDirection = LaunchDirection;
    function LaunchDirection()
    {
        this.StartString = "current location";
        this.destinationString = LocationData;
        this.FullURL = "http://maps.google.com/maps?saddr=" + this.StartString + "&daddr=" + this.destinationString;
        window.location = this.FullURL;
    }
}

