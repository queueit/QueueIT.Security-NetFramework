<%@ Page Title="Queue Landing Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AdvancedLanding.aspx.cs" Inherits="QueueIT.Security.Examples.Webforms.AdvancedLanding" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
                <h2>Split page.</h2>
            </hgroup>
            <p>
                To learn more about configuring queues, please contact Queue-it.
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
        <a href="Default.aspx">Back To Home</a> <asp:HyperLink ID="hlQueue" runat="server">Go to queue</asp:HyperLink>
</asp:Content>
