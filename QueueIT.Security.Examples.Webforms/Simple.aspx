<%@ Page Title="Simple Queue Configuration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Simple.aspx.cs" Inherits="QueueIT.Security.Examples.Webforms.Simple" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
                <h2>A simple configuration of a queue.</h2>
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
            This example uses the queue with name &#39;default&#39; from the web config file. The 
            entry contains the minimum required attributes.</li>
        <li class="two">
            <h5>Write controller code</h5>
            Add controller code to the PreInit event. The eventhandler of the Simple.aspx 
            page contains the minimum code required to set up the queue. You may place this 
            code in the page PreInit, in the master page PreInit or in the 
            Application_BeginRequest method of the global.asax file.</li>
        <li class="three">
            <h5>Add the SessionValidation Web Control </h5>
            Add the Queue-it SessionValidationControl to the buttom of all pages which runs 
            the validation code. You may place the control on specific pages or use master 
            pages (it must only be included once on each page). In this example you will 
            find the control on the Site.Master page.</li>
    </ol>
        <div>
        <asp:HyperLink ID="hlCancel" runat="server">Cancel queue validation token</asp:HyperLink>
        <asp:HyperLink ID="hlExpire" runat="server">Change expiration</asp:HyperLink>
    </div>
</asp:Content>
