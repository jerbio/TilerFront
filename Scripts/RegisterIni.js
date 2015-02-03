"use strict"

var registrationPageContainer = document.getElementById("regSignPageContainer");
$(document).ready(function () {
    registrationPageContainer = document.getElementById("regSignPageContainer");
    InitializeSignInReg();
});

function InitializeSignInReg()
{

    var gotoSignInScreenButton = document.getElementById("gotoSignInScreenButton");
    var gotToRegisterScreenButton = document.getElementById("gotToRegisterScreenButton");
    var SignInFormContainer = document.getElementById("SignInFormContainer");
    var RegPageFormContainer = document.getElementById("RegPageFormContainer");
    $(RegPageFormContainer).addClass("HomePageButton");
    $(SignInFormContainer).addClass("HomePageButton");

    var UserNameSignInDom = document.getElementById("UserNameSignin");
    var PasswordSigninDom = document.getElementById("PasswordSignin");
    $(UserNameSignInDom).on('input', CheckFocuseChange);
    $(PasswordSigninDom).on('input', CheckFocuseChange);



    



    
    var EmailReginDom = document.getElementById("EmailRegin"); $(EmailReginDom).focusout(checkIfEmailIsValid);

        
    var FullNameReginDom = document.getElementById("FullNameRegin"); //FullNameReginDom.onfocus = checkIfRegistrationValuesMakeSense;// $(FullNameReginDom).focus(function () { setTimeout(checkIfRegistrationValuesMakeSense, 1000) });
    var UserNameReginDom = document.getElementById("UserNameSignin"); //UserNameReginDom.onfocus = checkIfRegistrationValuesMakeSense; //$(UserNameReginDom).focus(function () { setTimeout(checkIfRegistrationValuesMakeSense, 1000) });
    var PasswordReginDom = document.getElementById("PasswordSignin"); $(PasswordReginDom).focusout(checkIfPasswordIsValid);// PasswordReginDom.onfocus = checkIfRegistrationValuesMakeSense; //
    var ConfirmPasswordReginDom = document.getElementById("ConfirmPasswordRegin"); $(ConfirmPasswordReginDom).focusout(checkIfPasswordIsValid);
    $(UserNameSignInDom).focusout(CheckFocuseChange);
    


    var SignInButton = document.getElementById("SignInButton");
    var RegisterButton = document.getElementById("gotToRegisterScreenButton");
    

    var RegPageFormContainer = document.getElementById("RegPageFormContainer");
    $(RegPageFormContainer).hide();

    var callBackprepSignInFunc=prepSignInFunc(UserNameSignInDom, PasswordSigninDom);
    SignInButton.addEventListener("click", callBackprepSignInFunc);
    var callBackprepRegisterButton = prepFunctionOnRegisterRequest(SignInButton, RegisterButton, null, callBackprepSignInFunc, null, FullNameReginDom, UserNameReginDom, PasswordReginDom, EmailReginDom, ConfirmPasswordReginDom);
    RegisterButton.addEventListener("click", callBackprepRegisterButton);
    $(PasswordSigninDom).keyup(function (e) {//catches enter key press
        if (e.keyCode === 13) {
            callBackprepSignInFunc();
        }
    });
    //$(SignInButton).click();
    //$(RegisterButton).click();


    var TitleText = getDomOrCreateNew("BigBackGroundText");
    //addEventgotoScreenButton(gotoSignInScreenButton, SignInFormContainer, TitleText);
    //addEventgotoScreenButton(gotToRegisterScreenButton, RegPageFormContainer, TitleText);
}


function CheckFocuseChange()
{
    var UserNameReginDom = document.getElementById("UserNameSignin"); //UserNameReginDom.onfocus = checkIfRegistrationValuesMakeSense; //$(UserNameReginDom).focus(function () { setTimeout(checkIfRegistrationValuesMakeSense, 1000) });
    var PasswordReginDom = document.getElementById("PasswordSignin");
    var SignInButton = document.getElementById("SignInButton");
    var registerButton = document.getElementById("gotToRegisterScreenButton");

    if ((UserNameReginDom.value != null && UserNameReginDom.value != "")) {
        $(registerButton).addClass("enabledButton");
    }
    else
    {
        $(registerButton).removeClass("enabledButton");
        $(registerButton).addClass("disabledButton");
        $(SignInButton).removeClass("enabledButton");
        $(SignInButton).addClass("disabledButton");
        return;
    }

    if ((PasswordReginDom.value != null && PasswordReginDom.value != "") && (UserNameReginDom.value != null && UserNameReginDom.value != "")) {
        $(SignInButton).removeClass("disabledButton");
        $(SignInButton).addClass("enabledButton");
        $(registerButton).addClass("enabledButton");
        var a = {};
    }
    else
    {
        $(SignInButton).removeClass("enabledButton");
        $(SignInButton).addClass("disabledButton");
        $(registerButton).addClass("enabledButton");
    }
}

function checkIfEmailIsValid()
{
    var EmailReginDom = document.getElementById("EmailRegin");
    var EmailErrorDom = document.getElementById("EmailError");
    var res = EmailReginDom.value.match(/\b[A-Za-z0-9._%+-]+@[A-Za-z0-9_%+-]+\.[A-Za-z0-9._%+-]+\b/g);
    if ((res == "")||(res==null)) {
        EmailReginDom.style.borderBottom = "solid red 2px";
        EmailErrorDom.innerHTML = "Invalid Email";
        return false;
    }
    else
    {
        EmailReginDom.style.borderBottom = "solid 10px rgba(0,0,0,1)";
        EmailErrorDom.innerHTML = "";
        return true;
    }
}




function checkIfPasswordIsValid()
{
    CheckFocuseChange();
    var PasswordReginDom = document.getElementById("PasswordSignin");
    var ConfirmPasswordReginDom = document.getElementById("ConfirmPasswordRegin");
    var PassWordErrorDom = document.getElementById("PassWordConfirmErrorContainer");

    if ((ConfirmPasswordReginDom.value != PasswordReginDom.value) && (ConfirmPasswordReginDom.value != "") && (PasswordReginDom.value != ""))
    {
        ConfirmPasswordReginDom.style.borderBottom = "solid red 2px";
        PassWordErrorDom.innerHTML = "These Passwords don't match. Try again?";
        return false;
    }
    else
    {
        ConfirmPasswordReginDom.style.borderBottom = "solid 4px rgba(0,0,0,1)";
        PassWordErrorDom.innerHTML = "";
        return true;
    }
    
}
function checkIfBothPasswordsAreSame(PasswordDom1,PasswordDom2)
{
    return PasswordDom1.value == PasswordDom2.value
    true;
}

function prepSignInFunc(UserNameDom, PasswordDom)
{
    function retFunction () {
        SignInUser(UserNameDom.value, PasswordDom.value);
    }
    
    
    return retFunction;
}

//RegisterUser(FullName, UserName, Password, Email)

function prepRegisterFunc(FullNameDom, UserNameDom, PasswordDom, EmailDom, ConfirmPasswordDOm) {
    return function () 
    {
        RegisterUser(FullNameDom.value, UserNameDom.value, PasswordDom.value,ConfirmPasswordDOm.value, EmailDom.value);
    }
}
//*/


function prepFunctionOnRegisterRequest(SignInButton, RegisterButton, ShowSignInFormCallBack, postSignInCallBack, postRegistrationDataCallBack, FullNameReginDom, UserNameReginDom, PasswordReginDom, EmailReginDom, ConfirmPasswordReginDom)
{
    var RegPageFormContainer = document.getElementById("RegPageFormContainer");
    var SignInInputContainer = document.getElementById("SignInInputContainer");
    var SignButtonContainer = document.getElementById("SignButtonContainer");
    var BigBackGroundText = document.getElementById("BigBackGroundText");
    
    function retValue() {

        $(RegPageFormContainer).show();
        RegPageFormContainer.style.top = "30%";
        RegPageFormContainer.style.height = "75%";
        SignInInputContainer.style.height = "30%";
        SignButtonContainer.style.top = "90%";
        SignInButton.innerHTML = "Back";
        BigBackGroundText.style.top = "50%";
        BigBackGroundText.style.marginTop = "-150px";

        
        if (ShowSignInFormCallBack == null)
        {
            postRegistrationDataCallBack = prepRegisterFunc(FullNameReginDom, UserNameReginDom, PasswordReginDom, EmailReginDom, ConfirmPasswordReginDom);
            ShowSignInFormCallBack = prepFunctionOnSignInRequest(SignInButton, RegisterButton, retValue, postRegistrationDataCallBack, postSignInCallBack);
        }


        SignInButton.removeEventListener("click", postSignInCallBack);
        SignInButton.addEventListener("click", ShowSignInFormCallBack);

        RegisterButton.removeEventListener("click", retValue);
        RegisterButton.addEventListener("click", postRegistrationDataCallBack);
    }

    return retValue;
}


function prepFunctionOnSignInRequest(SignInButton, RegistrationButton, displayRegisterCallBack,postRegisterCallBack, postSignInCallBack)
{
    var RegPageFormContainer = document.getElementById("RegPageFormContainer");
    var SignInInputContainer = document.getElementById("SignInInputContainer");
    var SignButtonContainer = document.getElementById("SignButtonContainer");
    var BigBackGroundText = document.getElementById("BigBackGroundText");
    function retValue()
    {
        $(RegPageFormContainer).hide();
        RegPageFormContainer.style.height = "0%";
        RegPageFormContainer.style.top = "30%";
        SignInInputContainer.style.height = "40%";
        SignButtonContainer.style.top = "80%";
        BigBackGroundText.style.height = "50%";
        SignInButton.innerHTML = "Sign In";
        BigBackGroundText.style.top = "0%";
        BigBackGroundText.style.marginTop = "0";

        RegistrationButton.removeEventListener("click", postRegisterCallBack);
        RegistrationButton.addEventListener("click", displayRegisterCallBack);


        SignInButton.removeEventListener("click", retValue);
        SignInButton.addEventListener("click", postSignInCallBack);

     }

     return retValue;
}


function addEventgotoScreenButton(ClickDom, ShowThisDOm,TitleText)
{
    $(ClickDom).click(function () {
        ShowThisDOm.style.left = "0%";
        ShowThisDOm.style.visibility = "visible";
        ClickDom.parentNode.style.left = "-100%";
        ClickDom.parentNode.style.visibility = "hidden";
        if (TitleText.status) {
            TitleText.Dom.innerHTML = "Sign Up";
            TitleText.status = false;
        }
        else
        {
            TitleText.Dom.innerHTML = "Sign in to WagTap";
            TitleText.status = true;
        }
})
}

function SignInUser(UserName,Password)
{
    var TimeZone = new Date().getTimezoneOffset();
    var LoginCredentials = { UserName: UserName, Password: Password ,TimeZoneOffset: TimeZone };
    var url = global_refTIlerUrl+ "User/SignIn";
    //url="RootWagTap/time.top?WagCommand=4";
    $.ajax({
        type: "POST",
        url: url,
        data: LoginCredentials,
        // DO NOT SET CONTENT TYPE to json
        // contentType: "application/json; charset=utf-8", 
        // DataType needs to stay, otherwise the response object
        // will be treated as a single string
        //dataType: "json",
        success: function (response) {
            //alert(response);
            var myContainer = response;//JSON.parse(response);
            if (myContainer.Error.code==0)
            {
                LaunchLoggedUser(myContainer.Content);
            }
            else
            {
                showSignInError();
            }
        },
        error: function (err) {
            var myError = err;
            var step = "err";
        }

    }).done(function (data) {
        
    });
}

function showSignInError()
{
    var SignInError = document.getElementById("SignInError");
    SignInError.style.visibility = "visible";
    SignInError.innerHTML = "Failed to Sign in. Invalid Password/UserName. Please Try Again?";
}

function showRegistrationError(message)
{
    var FormSubmissionError = document.getElementById("FormSubmissionError");
    FormSubmissionError.innerHTML = message;
    FormSubmissionError.style.visibility = "visible";
}

function hideRegistrationError()
{
    var FormSubmissionError = document.getElementById("FormSubmissionError");
    FormSubmissionError.style.visibility = "hidden";
}

function RegisterUser(FullName, UserName, Password,PassWordConfirmation, Email)
{
    var FullName = FullName.split(" ");

    if (!checkIfPasswordIsValid() || !checkIfEmailIsValid())
    {
        showRegistrationError("Please Check your registration credentials")
        return;
    }

    var TimeZone = new Date().getTimezoneOffset();

    var RegistrationCredentials = { UserName: UserName, Password: Password,ConfirmPassword:PassWordConfirmation, FirstName: FullName[0], LastName: FullName[1], Email: Email ,TimeZoneOffset: TimeZone };
    
    //var url="RootWagTap/time.top?WagCommand=3";
    var url=global_refTIlerUrl+"User/New";

    $.ajax({
        type: "POST",
        url: url,
        data: RegistrationCredentials,
        success: function (response) {
            //alert(response);
            var myContainer = response
            //myContainer= JSON.parse(myContainer);
            if (myContainer.Error.code == 0)
            {
                LaunchLoggedUser(myContainer.Content);
            }
            else
            {
                showRegistrationError(myContainer.Error.Message);
                setTimeout(hideRegistrationError, 6000);
            }
        },
        error: function (err) {
            var myError = err;
            var step = "err";
        }

    }).done(function (data) {

    });
}


function LaunchLoggedUser(myContainer)
{
    //"{ID:\"\",User:\"\"}"
    var cookieString = JSON.stringify( myContainer)
    SetCookie(cookieString);
    var urlString;
    if (myContainer.MobileFlag)
    {
        urlString = "LoggedInUser.html?User=" + myContainer.UserName + "&UserNumber=" + myContainer.UserID;
    }
    else
    {
        urlString = "MonthOverView.html?User=" + myContainer.UserName + "&UserNumber=" + myContainer.UserID;
    }
    

    //var urlString = "LoggedInUser.html?User=" + myContainer.User + "&UserNumber=" + myContainer.ID;
    window.location.href = urlString;
}


