$(function () {
    const token = document.querySelector('meta[name="RequestVerificationToken"]')?.content;
    if (token) $.ajaxSetup({ headers: { RequestVerificationToken: token } });
});
