<%@ Page Title="Queue-it" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="QueueIT.Security.Examples.Webforms._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
                <h2>Configuration of queues.</h2>
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
        </li>
        <li class="two">
            <h5>Write controller code</h5>
        </li>
    </ol>
</asp:Content>
