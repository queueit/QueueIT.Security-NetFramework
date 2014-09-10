<%@ Page Title="Code Only Queue Configuration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CodeOnly.aspx.cs" Inherits="QueueIT.Security.Examples.Webforms.CodeOnly" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
                <h2>Creating the queue in code.</h2>
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
            <h5>Add configuration using code</h5>
            All configuration that is supported using the configuration section is also 
            supported in code. In this example it is configured in the static constructor of 
            &#39;CodeOnly.aspx.cs&#39; for readability. You could also place it in the 
            Application_Start method in the global.asax file.</li>
        <li class="two">
            <h5>Write controller code</h5>
            Add controller code to the PreInit event. The eventhandler of the 
            CodeOnly.aspx 
            configures the queue with Customer ID and Event ID and thereby bypasses the 
            configuration section. You may place this 
            code in the page PreInit, in the master page PreInit or in the 
            Application_BeginRequest method of the global.asax file.</li>
    </ol>
    <asp:HyperLink ID="hlCancel" runat="server">Cancel queue validation token</asp:HyperLink>
    <asp:HyperLink ID="hlExpire" runat="server">Change expiration</asp:HyperLink>
</asp:Content>
