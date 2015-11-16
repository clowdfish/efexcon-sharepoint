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

    var inputIsMissing = false;
    $(".new-form :input").each(function () {
        if (!$(this).val()) {
            inputIsMissing = true;
        }
    });

    if (inputIsMissing) {
        $('#newFormStatus').text("All fields must be filled.");
        $('#newFormStatus').css('color', 'red');
        return false;
    }

    return true;
}