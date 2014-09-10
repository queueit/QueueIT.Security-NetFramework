<%@ Page Title="Advanced Queue Configuration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Advanced.aspx.cs" Inherits="QueueIT.Security.Examples.Webforms.Advanced" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
                <h2>An advanced configuration of a queue.</h2>
            </hgroup>
            <p>
                To learn more about configuring queues, please contact Queue-it.
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <h3>Setting up the queue:</h3>
    <ol class="round">
        <li class="one">
            <h5>Add configuration section to web config</h5>
            This example uses the queue with name &#39;advanced&#39; from the web config file. The 
            entry contains a domain alias which is used when users are redirected to the 
            queue as well as a landing page (split page) allowing users to choose if the 
            want to be redirected to the queue.</li>
        <li class="two">
            <h5>Write controller code</h5>
            Add controller code to the PreInit event. The eventhandler of the Advanced.aspx 
            page contains code to extract and persist information about a queue number. The 
            AdvancedLanding page contains code to route the user to the queue and back to 
            the Advanced.aspx page once the user has been through the queue. You may place this 
            code in the page PreInit, in the master page PreInit or in the 
            Application_BeginRequest method of the global.asax file.</li>
    </ol>
    <asp:HyperLink ID="hlCancel" runat="server">Cancel queue validation token</asp:HyperLink>
    <asp:HyperLink ID="hlExpire" runat="server">Change expiration</asp:HyperLink>
</asp:Content>
