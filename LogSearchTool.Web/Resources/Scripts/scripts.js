$(document).ready(function () {
    
    $(".conditional").hide();

    if ($("#matchedFilesSection").find("table").length > 0) {
        $(".conditional").show();
    }
    else {
        $(".conditional").hide();
    }

    if ($('#matchedContent').html()) {
        $('#matchedContent').focus();
    }

    setServerSectionDisplay();

    setIndServerDisplay();

    $("#appName").change(function (e) {

        setServerSectionDisplay();

        //        $.ajax({
        //            type: "POST",
        //            url: "Search.aspx?log=" + $("#appName").val(),
        //            data: "{selectedValue: '" + $("#appName").val() + "'}",
        //            success: function () {
        //                debugger;
        //                var val = $("#appName").val();
        //                if (val != null && val != "") {
        //                    $("#indServerSection").show();
        //                }
        //                else {
        //                    $("#indServerSection").hide();
        //                };

        //            }
        //        });
    });

    $("input[name='specificServer']").click(function () {
        setIndServerDisplay();
    });

    $("#reset").click(function () {
        $(':input', "form").each(function () {
            var type = this.type;
            var tag = this.tagName.toLowerCase(); // normalize case
            // it's ok to reset the value attr of text inputs,
            // password inputs, and textareas
            if (type == 'text' || type == 'password' || tag == 'textarea')
                this.value = "";
            // checkboxes and radios need to have their checked state cleared
            // but should *not* have their 'value' changed
            else if (type == 'checkbox')
                this.checked = false;
            // select elements need to have their 'selectedIndex' property set to -1
            // (this works for both single and multiple select elements)
            else if (tag == 'select')
                this.selectedIndex = 0;
        });
    });

    function setServerSectionDisplay() {
        var val = $("#appName").val();
        if (val != null && val != "") {
            $("#indServerSection").show();
        }
        else {
            $("#indServerSection").hide();
        };
    }

    function setIndServerDisplay() {
        var val = $("#specificServer_1").attr("checked");
        if (val == 'checked') {
            $("#individualServer").show();
        }
        else {
            $("#individualServer").hide();
        }
    }
});

