using System.Threading.Tasks;
using Dropbox.Api.Files;

namespace ParanoidDropboxBackup.Dropbox
{
    public class CalcSizeContentVisitor : IContentVisitor
    {
        public ulong TotalSize;

        public Task Visit(FileMetadata file)
        {
            TotalSize += file.Size;
            return Task.CompletedTask;
        }

        public Task Visit(FolderMetadata folder)
        {
            return Task.CompletedTask;
        }

        public Task Visit(DeletedMetadata deleted)
        {
            return Task.CompletedTask;
        }
    }
}