$(function checkRegistration() {
    var usernameState, emailState, passwordState, cPasswordState = false;
    buttonAvailability();
    $("#Username").blur(function() {
        var username = $("#Username").val();
        usernameState = false;
        buttonAvailability();
        $("#Username").next().hide().text("");

        if (username == "") {
            $("#Username").next().hide().text("Данное поле обязательно для заполнения").css("color", "red").fadeIn(400);
            $("#Username").removeClass().addClass('error');
            return;
        }

        var pattern = /^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]$/;
        if (pattern.test(username)) {
            $("#Username").next().hide().text("Некорректный логин").css("color", "red").fadeIn(400);
            $("#Username").removeClass().addClass('error');
            return;
        }
        $.ajax({
            url: "/Account/CheckUsername",
            data: { Username: username },
            success: function(response) {
                if (response == false) {
                    $("#Username").next().hide().text("Данный логин уже используется").css("color", "red").fadeIn(400);
                    $("#Username").removeClass().addClass('error');
                    return;
                } else {
                    $("#Username").removeClass().addClass('ok');
                    usernameState = true;
                    buttonAvailability();
                }
            }
        });
    });

    $("#Email").blur(function() {
        var email = $("#Email").val();
        emailState = false;
        buttonAvailability();
        $("#Email").next().hide().text("");
        if (email == "") {
            $("#Email").next().hide().text("Данное поле обязательно для заполнения").css("color", "red").fadeIn(400);
            $("#Email").removeClass().addClass('error');
            return;
        }
        var pattern = /^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;
        if (!pattern.test(email)) {
            $("#Email").next().hide().text("Некорректный адрес").css("color", "red").fadeIn(400);
            $("#Email").removeClass().addClass('error');
            return;
        }

        $.ajax({
            url: "/Account/CheckEmail",
            data: { Email: email },
            success: function (response) {
                if (response == false) {
                    $("#Email").next().hide().text("Данная почта уже используется").css("color", "red").fadeIn(400);
                    $("#Email").removeClass().addClass('error');
                    return;
                }
                else {
                    $("#Email").removeClass().addClass('ok');
                    emailState = true;
                    buttonAvailability();
                }
            }
        });
    });

    $("#Password").blur(function () {
        var password = $("#Password").val();
        passwordState = false;
        buttonAvailability();
        $("#Password").next().hide().text("");
        if (password == "") {
            $("#Password").next().hide().text("Данное поле обязательно для заполнения").css("color", "red").fadeIn(400);
            $("#Password").removeClass().addClass('error');
            return;
        }
        if (password.length < 6 || password.length >= 100) {
            $("#Password").next().hide().text("Пароль должен иметь от 6 до 100 символов").css("color", "red").fadeIn(400);
            $("#Password").removeClass().addClass('error');
            return;
        }
        $("#Password").removeClass().addClass('ok');
        passwordState = true;
        buttonAvailability();
        if ($("#ConfirmPassword").val != "") {
            passwordComparison();
        }
    });

    $("#ConfirmPassword").blur(passwordComparison);

    function passwordComparison() {
        var cPassword = $("#ConfirmPassword").val();
        cPasswordState = false;
        buttonAvailability();
        $("#ConfirmPassword").next().hide().text("");
        if (cPassword == "") {
            $("#ConfirmPassword").removeClass().addClass('error');
            return;
        }
        if (cPassword.localeCompare($("#Password").val())) {
            $("#ConfirmPassword").next().hide().text("Пароли должны совпадать").css("color", "red").fadeIn(400);;
            $("#ConfirmPassword").removeClass().addClass('error');
            return;
        }
        $("#ConfirmPassword").removeClass().addClass('ok');
        cPasswordState = true;
        buttonAvailability();
    }

    function buttonAvailability() {
        if (emailState && passwordState && cPasswordState && usernameState) {
            $("#Button").removeAttr("disabled");
        } else {
            $("#Button").attr("disabled", "disabled");
        }
    }
})