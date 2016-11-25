using System;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.Mail;
using SmtpServer.Tests.Mocks;
using Xunit;
using MailKit.Net.Smtp;
using MimeKit;

namespace SmtpServer.Tests
{
    public class SmtpServerTests
    {
        readonly MockMessageStore _messageStore;
        readonly OptionsBuilder _optionsBuilder;
        // Estudar:
        // https://johnbadams.wordpress.com/2012/03/10/understanding-cancellationtokensource-with-tasks/
        // https://msdn.microsoft.com/en-us/library/dd997364.aspx
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public SmtpServerTests()
        {
            _messageStore = new MockMessageStore();

            _optionsBuilder = new OptionsBuilder()
                .ServerName("localhost")
                .Port(25)
                .MessageStore(_messageStore);
        }

        [Fact]
        public void CanReceiveMessage()
        {
            // arrange
            var smtpServer = new SmtpServer(_optionsBuilder.Build());
            var smtpServerTask = smtpServer.StartAsync(_cancellationTokenSource.Token);

            // act
            using (var client = new SmtpClient())
            {
                var mimeMessage = new MimeKit.MimeMessage();
                mimeMessage.From.Add(new MailboxAddress("Test", "test1@test.com"));
                mimeMessage.To.Add(new MailboxAddress("Destinatary", "test2@test.com"));
                mimeMessage.Subject = "Test";
                mimeMessage.Body = new TextPart("plain")
                {
                    Text = "Test message to server"
                };

                client.Connect("localhost", 25);
                client.Send(mimeMessage);
                
                // Assert
                Assert.Equal(1, _messageStore.Messages.Count);
                Assert.Equal("test1@test.com", _messageStore.Messages[0].From.AsAddress());
                Assert.Equal(1, _messageStore.Messages[0].To.Count);
                Assert.Equal("test2@test.com", _messageStore.Messages[0].To[0].AsAddress());

                // Some code of SmtpClient disposes _cancellationTokenSource and throw exception if run wait code.
                // Without SMTP Client code works well.
                //Wait(smtpServerTask);

                client.Disconnect(true);
            }
        }

        void Wait(Task smtpServerTask)
        {
            _cancellationTokenSource.Cancel();
            try
            {
                smtpServerTask.Wait();
            }
            catch (AggregateException e)
            {
                e.Handle(exception => exception is OperationCanceledException);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [Fact]
        public void RunningTaskDoesNotRespondToCancel_ButActionDoes()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            Task taskWithToken = new Task(
                () =>
                {
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }, token
            );
            taskWithToken.Start();
            while (taskWithToken.Status != TaskStatus.Running)
            {
                //Wait until task is running
            }
            tokenSource.Cancel();  //cancel...
            taskWithToken.Wait();  //...and wait for the action within the task to complete
            Assert.False(taskWithToken.Status == TaskStatus.Canceled);
            Assert.True(taskWithToken.Status == TaskStatus.RanToCompletion);
        }
        [Fact]
        public void RunningTaskDoesNotContinueToCancel()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            Task taskWithToken = new Task(
                () =>
                {
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }, token
            );
            var canceledTask = taskWithToken.ContinueWith(
                (antecedentTask) => Assert.True(false, "Canceled"),
                TaskContinuationOptions.OnlyOnCanceled);
            var completedTask = taskWithToken.ContinueWith(
                (antecedentTask) => Assert.True(true),
                TaskContinuationOptions.OnlyOnRanToCompletion);
            taskWithToken.Start();
            while (taskWithToken.Status != TaskStatus.Running)
            {
                //Wait until task is running
            }
            tokenSource.Cancel();
            taskWithToken.Wait();
            completedTask.Wait();
            // The completedTask continuation should have run to completion
            // since its antecendent task (taskWithToken) also ran to completion.
            Assert.True(completedTask.Status == TaskStatus.RanToCompletion);
            // The canceledTask continuation should have been canceled since
            // its antecedent task (taskWithToken) ran to completion.
            Assert.True(canceledTask.Status == TaskStatus.Canceled);
        }
        [Fact]
        public void NonRunningTaskDoesRespondToCancel_AndActionDoesToo()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            Task taskWithToken = new Task(
                () =>
                {
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }, token
            );
            taskWithToken.Start();
            if (taskWithToken.Status != TaskStatus.Running)
            {
                tokenSource.Cancel();  //cancel...
                bool taskThrewExceptionUponWait = false;
                try
                {
                    taskWithToken.Wait();  //...and wait for the action within the task to complete
                }
                catch (Exception ex)
                {
                    Assert.Equal(ex.GetType(), typeof(AggregateException));
                    var inner = ((AggregateException)ex).InnerExceptions[0];
                    Assert.Equal(inner.GetType(), typeof(TaskCanceledException));
                    taskThrewExceptionUponWait = true;
                }
                Assert.True(taskWithToken.Status == TaskStatus.Canceled);
                Assert.True(taskThrewExceptionUponWait);
            }
            else
            {
                try
                {
                    tokenSource.Cancel(); // Clean up
                    Assert.False(true, "Task was already running when cancel would have fired");
                }
                catch { }
            }
        }
        [Fact]
        public void NonRunningTaskDoesContinueToCancel()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            Task taskWithToken = new Task(
                () =>
                {
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }, token
            );
            var canceledTask = taskWithToken.ContinueWith(
                (antecedentTask) => Assert.True(true),
                TaskContinuationOptions.OnlyOnCanceled);
            var completedTask = taskWithToken.ContinueWith(
                (antecedentTask) => Assert.False(true, "Completed"),
                TaskContinuationOptions.OnlyOnRanToCompletion);
            taskWithToken.Start();
            if (taskWithToken.Status != TaskStatus.Running)
            {
                tokenSource.Cancel();
                try
                {
                    taskWithToken.Wait();
                }
                catch
                {
                    // Not interested in the exceptions for this test
                }
                canceledTask.Wait();
                // The completedTask continuation should have been canceled
                // since its antecedent task (taskWithToken) was canceled
                // and did not run to completion.
                Assert.True(completedTask.Status == TaskStatus.Canceled);
                // The canceledTask continuation should have run to completion
                // since its antecendent task (taskWithToken) was canceled.
                Assert.True(canceledTask.Status == TaskStatus.RanToCompletion);
            }
            else
            {
                try
                {
                    tokenSource.Cancel(); // Clean up
                    Assert.True(true, "Task was already running when cancel would have fired");
                }
                catch { }
            }
        }
        [Fact]
        public void CanceledTaskCanNotBeStarted()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            Task taskWithToken = new Task(
                () =>
                {
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }, token
            );
            tokenSource.Cancel();
            bool runtimeThrewException = false;
            try
            {
                taskWithToken.Start();
            }
            catch (Exception ex)
            {
                Assert.Equal(ex.GetType(), typeof(InvalidOperationException));
                runtimeThrewException = true;
            }
            Assert.True(runtimeThrewException);
        }
        [Fact]
        public void TaskWithoutTokenReportsNotCanceled()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Task taskWithToken = new Task(
                () =>
                {
                    while (true)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                } // No Token
            );
            taskWithToken.Start();
            tokenSource.Cancel();
            taskWithToken.Wait();
            Assert.False(taskWithToken.IsCanceled);
            Assert.True(taskWithToken.Status == TaskStatus.RanToCompletion);
        }
    }
}
