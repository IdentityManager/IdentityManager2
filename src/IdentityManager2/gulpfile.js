var gulp = require("gulp"),
    fs = require("fs"),
    concat = require("gulp-concat"),
    less = require("gulp-less"),
    cleanCSS = require('gulp-clean-css');

gulp.task("css", function () {
    return gulp.src('Assets/Content/Style.less')
        .pipe(less())
        .pipe(concat('Assets/Content/Style.css'))
        .pipe(gulp.dest('.'))
        .pipe(cleanCSS())
        .pipe(concat('Assets/Content/Style.min.css'))
        .pipe(gulp.dest('.'));
});

gulp.task("js", function () {
    return gulp.src([
            'Assets/Scripts/Libs/jquery-2.1.0.min.js',
            'Assets/Scripts/Libs/bootstrap.min.js',
            'Assets/Scripts/Libs/angular.min.js',
            'Assets/Scripts/Libs/angular-route.min.js',
            'Assets/Scripts/Libs/oidc-token-manager.min.js',
            'Assets/Scripts/App/ttIdm.js',
            'Assets/Scripts/App/ttIdmUI.js',
            'Assets/Scripts/App/ttIdmUsers.js',
            'Assets/Scripts/App/ttIdmRoles.js',
            'Assets/Scripts/App/ttIdmApp.js'
        ])
        .pipe(concat('Assets/Scripts/Bundle.js'))
        .pipe(gulp.dest('.'));
});
