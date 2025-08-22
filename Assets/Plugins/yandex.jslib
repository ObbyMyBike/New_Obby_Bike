mergeInto(LibraryManager.library, {

  ShowFullscreen: function () {
      ysdk.adv.showFullscreenAdv({
      callbacks: {
          onOpen: function(wasShown) {
            myGameInstance.SendMessage("Pause", "Paus");
            console.log('open');
          },
          onClose: function(wasShown) {
            console.log("close");
          },
          onError: function(error) {
            console.log("error");
          }
      }
      })
  },
})