<%@ Page Title="Expire Validation" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Expire.aspx.cs" Inherits="QueueIT.Security.Examples.Webforms.Expire" %>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
                <h2>Cancelation of validation result.</h2>
            </hgroup>
            <p>
                To learn more about cancelation of validation results, please contact Queue-it.
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Expire validation result</h3>
    <p>Your validation result has set to expire in 15 sec</p>
</asp:Content>
