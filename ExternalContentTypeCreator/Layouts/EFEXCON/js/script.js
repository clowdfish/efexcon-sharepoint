function showNewForm(button) {
    $('.new-form').fadeIn();
    $(button).hide();
}

function checkForm() {
    $('#newFormStatus').text("");

    var title = $('#title').val();
    var connectionString = $('#connectionString').val();
    var username = $('#username').val();
    var password = $('#password').val();

    if (title && connectionString && username && password)
        return true;

    $('#newFormStatus').text("All fields must be filled.");
    $('#newFormStatus').css('color', 'red');
    return false;
}