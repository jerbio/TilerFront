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
    $(PasswordReginDom).keyup(function (e) {//catches enter key press
        if(e.target.value) {
            let passwordEntry = onPasswordEntry(e.target.value);
            let PasswordInstructionDom = getDomOrCreateNew("PasswordInstruction");
            let passwordMinLengthDom = getDomOrCreateNew("passwordMinLength");
            let passwordNeedsUpperCaseDom = getDomOrCreateNew("passwordNeedsUpperCase");
            let passwordNeedsLowerCaseDom = getDomOrCreateNew("passwordNeedsLowerCase");
            let passwordNeedsNumberDom = getDomOrCreateNew("passwordNeedsNumber");
            let passwordNeedsNoneLetterAndNoneNumberDom = getDomOrCreateNew("passwordNeedsNoneLetterAndNoneNumber");
            if (passwordEntry.isValid) {
                $(PasswordInstructionDom).addClass('setAsDisplayNone');
            } else {
                $(PasswordInstructionDom).removeClass('setAsDisplayNone');
                if(passwordEntry.hasLowerCase) {
                    $(passwordNeedsLowerCaseDom).addClass('setAsDisplayNone');
                } else {
                    $(passwordNeedsLowerCaseDom).removeClass('setAsDisplayNone');
                }
    
                if(passwordEntry.hasUpperCase) {
                    $(passwordNeedsUpperCaseDom).addClass('setAsDisplayNone');
                } else {
                    $(passwordNeedsUpperCaseDom).removeClass('setAsDisplayNone');
                }
    
                if(passwordEntry.hasNumber) {
                    $(passwordNeedsNumberDom).addClass('setAsDisplayNone');
                } else {
                    $(passwordNeedsNumberDom).removeClass('setAsDisplayNone');
                }
    
                if(passwordEntry.isLongEnough) {
                    $(passwordMinLengthDom).addClass('setAsDisplayNone');
                } else {
                    $(passwordMinLengthDom).removeClass('setAsDisplayNone');
                }

                if(passwordEntry.isNoneLetterAndNoneNumber) {
                    $(passwordNeedsNoneLetterAndNoneNumberDom).addClass('setAsDisplayNone');
                } else {
                    $(passwordNeedsNoneLetterAndNoneNumberDom).removeClass('setAsDisplayNone');
                }
            }
        }

    });

    var TitleText = getDomOrCreateNew("BigBackGroundText");
}


function CheckFocuseChange()
{
    var UserNameReginDom = document.getElementById("UserNameSignin"); //UserNameReginDom.onfocus = checkIfRegistrationValuesMakeSense; //$(UserNameReginDom).focus(function () { setTimeout(checkIfRegistrationValuesMakeSense, 1000) });
    var PasswordReginDom = document.getElementById("PasswordSignin");
    var SignInButton = document.getElementById("SignInButton");
    var registerButton = document.getElementById("gotToRegisterScreenButton");

    if ((UserNameReginDom.value !== null && UserNameReginDom.value !== "")) {
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

    if ((PasswordReginDom.value !== null && PasswordReginDom.value !== "") && (UserNameReginDom.value !== null && UserNameReginDom.value !== "")) {
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
    if ((res === "")||(res === null)) {
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

function onPasswordEntry(passwordString) {
    let minPasswordLength = 6;

    function isPasswordLongEnough(passwordString) {
        let retValue = false;
        
        if(isString(passwordString) && passwordString.length > minPasswordLength) {
            retValue = true;
        }
        return retValue;
    }

    function passwordHasUpperCase(passwordString) {
        let hasUpperCase = /[A-Z]+/;
        let retValue = hasUpperCase.test(passwordString);
        return retValue;
    }

    function passwordHasLowerCase(passwordString) {
        let hasLowerCase = /[a-z]+/;
        let retValue = hasLowerCase.test(passwordString);
        return retValue;
    }

    function passwordHasNumber(passwordString) {
        let hasNumber = /[0-9]+/;
        let retValue = hasNumber.test(passwordString);
        return retValue;
    }

    function isNoneLetterAndNoneNumber(passwordString) {
        let hasNumber = /[^a-zA-Z0-9]/;
        let retValue = hasNumber.test(passwordString);
        return retValue;
    }

    let passwordIsValid = isPasswordLongEnough(passwordString) && passwordHasUpperCase(passwordString) && passwordHasLowerCase(passwordString) && passwordHasNumber(passwordString) && isNoneLetterAndNoneNumber(passwordString);

    let retValue = {
        isLongEnough: isPasswordLongEnough(passwordString),
        hasUpperCase: passwordHasUpperCase(passwordString),
        hasLowerCase: passwordHasLowerCase(passwordString),
        hasNumber: passwordHasNumber(passwordString),
        hasNonNumberAndNonLetter: passwordHasNumber(passwordString),
        isNoneLetterAndNoneNumber: isNoneLetterAndNoneNumber(passwordString),
        isValid: passwordIsValid
    };

    return retValue;
}


function checkIfPasswordIsValid()
{
    CheckFocuseChange();
    var PasswordReginDom = document.getElementById("PasswordSignin");
    var ConfirmPasswordReginDom = document.getElementById("ConfirmPasswordRegin");
    var PassWordErrorDom = document.getElementById("PassWordConfirmErrorContainer");

    if ((ConfirmPasswordReginDom.value !== PasswordReginDom.value) && (ConfirmPasswordReginDom.value !== "") && (PasswordReginDom.value !== ""))
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
    return PasswordDom1.value === PasswordDom2.value;
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
    return function () {
        RegisterUser(FullNameDom.value, UserNameDom.value, PasswordDom.value,ConfirmPasswordDOm.value, EmailDom.value);
    };
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
        BigBackGroundText.style.color = "rgba(200,200,200,.5)";
        BigBackGroundText.style.marginTop = "-150px";

        
        if (ShowSignInFormCallBack === null)
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
        BigBackGroundText.style.color = "rgba(0,0,0,1)";

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


    var __RequestVerificationToken = $('input[name=__RequestVerificationToken]').val();
    var LoginCredentials = { Username: UserName, Password: Password, __RequestVerificationToken: __RequestVerificationToken };
    var url = window.location.origin + "/Account/SignIn";
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
            debugger;
            let tokenPromise = configureAuthorizationToken(UserName, Password);
            tokenPromise.then((tokenResponse) => {
                let myContainer = response
                if (myContainer.Error.code === "0") {
                    LaunchLoggedUser(myContainer.Error.Message);
                }
                else {
                    showRegistrationError(myContainer.Error.Message);
                    setTimeout(hideRegistrationError, 6000);
                }
            })
            
        },
        error: function (err) {
            //debugger;

            showRegistrationError(err);
            setTimeout(hideRegistrationError, 6000);
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
    FullName = FullName.split(" ");

    if (!checkIfPasswordIsValid() || !checkIfEmailIsValid())
    {
        showRegistrationError("Please Check your registration credentials")
        return;
    }

    var TimeZone = new Date().getTimezoneOffset();

    var __RequestVerificationToken = $('input[name=__RequestVerificationToken]').val();
    //var LoginCredentials = { Username: UserName, Password: Password, };
    let firstName = FullName[0];
    let lastName = FullName[1];

    var RegistrationCredentials = { Username: UserName, Password: Password, ConfirmPassword: PassWordConfirmation, FirstName: firstName, LastName: lastName, Email: Email, __RequestVerificationToken: __RequestVerificationToken, TimeZoneOffSet: TimeZone };
    
    //var url="RootWagTap/time.top?WagCommand=3";
    //var url = global_refTIlerUrl + "User/New";
    var url = window.location.origin + "/Account/SignUp";
    preSendRequestWithLocation(RegistrationCredentials);
    debugger;
    $.ajax({
        type: "POST",
        url: url,
        data: RegistrationCredentials,
        success: function (response) {
            var myContainer = response
            let tokenPromise = configureAuthorizationToken(UserName, Password);
            tokenPromise.then((tokenResponse) => {
                let myContainer = response
                if (myContainer.Error.code === "0") {
                    LaunchLoggedUser(myContainer.Error.Message);
                }
                else {
                    showRegistrationError(myContainer.Error.Message);
                    setTimeout(hideRegistrationError, 6000);
                }
            })
        },
        error: function (err) {
            alert(err);
            showRegistrationError(err);
            setTimeout(hideRegistrationError, 6000);
        }

    }).done(function (data) {

    });
}


function LaunchLoggedUser(urlString)
{
    window.location.href = urlString;
}


