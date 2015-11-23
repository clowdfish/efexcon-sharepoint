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

    var checkboxArray = [];
    $(".data-source-structure-table input[type=checkbox]").each(function () {
        var name = $(this).attr("name");
        //name = name.substr(0, name.length - 6);
        var checked = $(this).is(':checked');

        if (checked)
            checkboxArray.push(name);
    });

    console.log(JSON.stringify(checkboxArray, null, 2));

    var selectedKeys =
        checkboxArray.filter(function (checkbox) {
            if (checkbox.indexOf("_key") === checkbox.length - 4) return true;
        });

    if (selectedKeys.length === 0) {
        alert("You must select one column as key.");
        return false;
    }

    if (selectedKeys.length > 1) {
        alert("You must not select more than one column as key.");
        return false;
    }

    var selectedKey = selectedKeys[0];

    var elementIsIncluded = checkboxArray.filter(function (checkbox) {
        if (checkbox.indexOf("_check") === checkbox.length - 6) {
            if (checkbox.substr(0, checkbox.length - 6) == selectedKey.substr(0, selectedKey.length - 4))
                return true;
        }
    }).length === 1;

    if (!elementIsIncluded) {
        alert("You must include the key column in the ECT.");
        return false;
    }

    $(".data-source-structure-table input[type=text]").each(function () {
        var name = $(this).attr("name");
        var value = $(this).val();
    });

    return true;
}
 
$(document).ready(function () {    
    $(".status-show-details").click(function () {
        $(this).hide();
        $(".status-details").fadeIn();
    });
});