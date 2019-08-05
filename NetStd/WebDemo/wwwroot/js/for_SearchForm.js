// JavaScript for the search form
//
// This disables the Fuzziness slider and sets it to zero when the Fuzzy checkbox
// is not checked

function ShowDateSearch() {
    var show = false;
    if (($('#EnableDateSearch') !== null) && $('#EnableDateSearch').is(':checked'))
        $('#FileDateDiv').show();
    else
        $('#FileDateDiv').hide();
}

function ShowFuzziness() {
    if (($('#Fuzzy') !== null) && $('#Fuzzy').is(':checked'))
        $('#FuzzinessDiv').show();
    else
        $('#FuzzinessDiv').hide();
}

function ShowIfChecked(cbName, divName) {
    if (($(cbName) !== null) && $(cbName).is(':checked'))
        $(divName).show();
    else
        $(divName).hide();
}


$(document).ready(function () {
    ShowIfChecked('#Fuzzy', '#FuzzinessDiv');
    ShowIfChecked('#EnableDateSearch', '#FileDateDiv');
    
    $('#EnableDateSearch').change(function () {
        if (this.checked)
            $('#FileDateDiv').show('fast');
        else
            $('#FileDateDiv').hide('fast');
    });

    $('#Fuzzy').change(function () {
        if (this.checked)
            $('#FuzzinessDiv').show('fast');
        else
            $('#FuzzinessDiv').hide('fast');
    });

    $('#AdvancedButton').click(function () {
        $(this).find("span").toggleClass('glyphicon-collapse-down').toggleClass('glyphicon-collapse-up');
    });


});

