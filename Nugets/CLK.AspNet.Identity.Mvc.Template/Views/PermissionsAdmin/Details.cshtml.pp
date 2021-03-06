﻿@model $rootnamespace$.Models.ApplicationPermission

@{
    ViewBag.Title = "Details";
}

<h2>Details.</h2>
<br />

<h4>This permission</h4>
<table class="table">
    <tr>
        <td style="width:80pt;">
            @Html.DisplayNameFor(model => model.Name)
        </td>
        <td>
            @Html.DisplayFor(model => model.Name)
        </td>
    </tr>
</table><br />

<h4>List of roles for this permission</h4>
@if (ViewBag.RoleNames.Count == 0)
{
    <hr />
    <p>No roles found for this permission.</p>
}
<table class="table">
    @foreach (var item in ViewBag.RoleNames)
    {
        <tr>
            <td>
                @item
            </td>
        </tr>
    }
</table><br />


<p>
    @Html.ActionLink("Edit", "Edit", new { id = Model.Id }) |
    @Html.ActionLink("Back to List", "Index")
</p>



