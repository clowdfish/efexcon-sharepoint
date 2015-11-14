function showNewForm() {
    $('.new-form').fadeIn();
    $('#ShowNewFormButton').hide();
}

function hideNewForm() {
    $('.new-form').hide();
    $('#ShowNewFormButton').show();
}

function checkForm() {
    $('#newFormStatus').text("");

    var title = $('#title').val();
    var url = $('#url').val();
    var database = $('#database').val();
    var username = $('#username').val();
    var password = $('#password').val();

    // check url format and return false if invalid

    if (title && url && database && username && password)
        return true;

    $('#newFormStatus').text("All fields must be filled.");
    $('#newFormStatus').css('color', 'red');
    return false;
}