function check_log_button() {
    App.btnLogin.setDisabled(!(App.txtUsername.getValue() && App.txtPassword.getValue()));
    return true;
};

function keyPress(item ,e) {
    var z = 0;
}