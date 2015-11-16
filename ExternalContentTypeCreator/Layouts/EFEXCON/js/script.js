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

function validStructure() {
    $(".data-source-structure-table input[type=checkbox]").each(function () {
        var name = $(this).attr("name");
        name = name.substr(0, name.length - 6);
        var checked = $(this).is(':checked');
    });

    $(".data-source-structure-table input[type=text]").each(function () {
        var name = $(this).attr("name");
        var value = $(this).val();
    });

    return true;
}