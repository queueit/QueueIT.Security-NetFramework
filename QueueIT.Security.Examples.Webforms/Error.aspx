<%@ Title="Error Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="QueueIT.Security.Examples.Webforms.Error" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
                <h2>There was an error validating your request.</h2>
            </hgroup>
            <p>
                To learn more about configuring queues, please contact Queue-it.
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <div>An error occured.</div>
    <div>
        <a href="Default.aspx">Back To Home</a> <asp:HyperLink ID="hlQueue" runat="server">Go to queue</asp:HyperLink>
    </div>
</asp:Content>
